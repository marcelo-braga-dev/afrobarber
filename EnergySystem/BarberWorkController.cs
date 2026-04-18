using UnityEngine;

public class BarberWorkController : MonoBehaviour
{
    [Header("Consumo de energia por atendimento")]
    [SerializeField] private float baseEnergyCost = 6f;
    [SerializeField] private float difficultyMultiplier = 1f;
    [SerializeField] private float serviceReceivedValue = 35f;
    [SerializeField] private string currentServiceName = "Serviço padrão";

    [Header("Qualidade manual do jogador")]
    [SerializeField] private float playerSkillBonus = 0f;

    [Header("UI Opcional")]
    [SerializeField] private ServiceEvaluationUI serviceEvaluationUI;

    private ServiceSessionData currentSession;

    public void StartService(ClientNPC clientNPC, float expectedDurationMinutes, float equipmentQuality, float productQuality, float comfortScore)
    {
        if (clientNPC == null || PlayerEnergySystem.Instance == null || BarberQueueSystem.Instance == null)
            return;

        currentSession = new ServiceSessionData();
        currentSession.clientNPC = clientNPC;
        currentSession.clientName = clientNPC.gameObject.name;
        currentSession.serviceStartGameMinutes = GameTimeSystem.Instance != null ? GameTimeSystem.Instance.TotalMinutesElapsed : 0f;

        ClientQueueData queueData = BarberQueueSystem.Instance.GetClientData(clientNPC);

        if (queueData != null)
        {
            currentSession.clientArrivalGameMinutes = queueData.arrivalGameMinutes;
            currentSession.waitingMinutes = queueData.GetWaitingMinutes(currentSession.serviceStartGameMinutes);
        }

        currentSession.expectedServiceDurationMinutes = expectedDurationMinutes;
        currentSession.maxAcceptableServiceDurationMinutes = expectedDurationMinutes * 1.25f;
        currentSession.equipmentQualityScore = equipmentQuality;
        currentSession.productQualityScore = productQuality;
        currentSession.environmentComfortScore = comfortScore;
        currentSession.barberEnergyAtStart = PlayerEnergySystem.Instance.CurrentEnergy;
    }

    public float GetAdjustedServiceDuration(float baseDurationMinutes)
    {
        if (PlayerEnergySystem.Instance == null)
            return baseDurationMinutes;

        return baseDurationMinutes * PlayerEnergySystem.Instance.GetServiceTimeMultiplier();
    }

    public float CalculateManualServiceQuality(float equipmentQuality, float productQuality)
    {
        float baseQuality = (equipmentQuality + productQuality) * 0.5f;

        if (PlayerEnergySystem.Instance != null)
            baseQuality -= PlayerEnergySystem.Instance.GetServiceQualityPenalty();

        baseQuality += playerSkillBonus;

        return Mathf.Clamp(baseQuality, 0f, 5f);
    }

    public ServiceEvaluationResult FinishService(float actualDurationMinutes, float equipmentQuality, float productQuality, bool hadMistakes)
    {
        if (currentSession == null || ClientEvaluationSystem.Instance == null || PlayerEnergySystem.Instance == null)
            return null;

        currentSession.serviceEndGameMinutes = GameTimeSystem.Instance != null ? GameTimeSystem.Instance.TotalMinutesElapsed : 0f;
        currentSession.actualServiceDurationMinutes = actualDurationMinutes;
        currentSession.barberEnergyAtEnd = PlayerEnergySystem.Instance.CurrentEnergy;
        currentSession.hadMistakes = hadMistakes;
        currentSession.clientWasServed = true;
        currentSession.usedGoodProducts = productQuality >= 3.5f;
        currentSession.manualServiceQualityScore = CalculateFinalQualityScore(equipmentQuality, productQuality, hadMistakes);

        float energyCost = baseEnergyCost * difficultyMultiplier;

        if (actualDurationMinutes > currentSession.expectedServiceDurationMinutes)
        {
            float overtime = actualDurationMinutes - currentSession.expectedServiceDurationMinutes;
            energyCost += overtime * 0.18f;
        }

        if (currentSession.expectedServiceDurationMinutes >= 25f)
            energyCost += 1.5f;

        PlayerEnergySystem.Instance.ConsumeEnergy(energyCost);
        PlayerEnergySystem.Instance.AddFatigue(energyCost * 0.8f);

        ServiceEvaluationResult result = ClientEvaluationSystem.Instance.EvaluateService(currentSession);

        if (ServiceHistorySystem.Instance != null && GameTimeSystem.Instance != null)
        {
            ServiceHistorySystem.Instance.AddEntry(
                day: GameTimeSystem.Instance.CurrentDay,
                timeOfDayMinutes: GameTimeSystem.Instance.CurrentTimeOfDayMinutes,
                clientName: currentSession.clientName,
                receivedValue: serviceReceivedValue,
                finalRating: result.finalScore,
                serviceName: currentServiceName
            );
        }

        if (serviceEvaluationUI != null)
            serviceEvaluationUI.Show(result);

        currentSession = null;
        return result;
    }

    public void SetCurrentServiceInfo(string serviceName, float receivedValue)
    {
        currentServiceName = serviceName;
        serviceReceivedValue = receivedValue;
    }

    private float CalculateFinalQualityScore(float equipmentQuality, float productQuality, bool hadMistakes)
    {
        float value = CalculateManualServiceQuality(equipmentQuality, productQuality);

        if (hadMistakes)
            value -= 1f;

        return Mathf.Clamp(value, 0f, 5f);
    }
}