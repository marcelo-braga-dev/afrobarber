using UnityEngine;

public class BarberWorkController : MonoBehaviour
{
    [Header("Consumo de energia por atendimento")]
    [SerializeField] private float baseEnergyCost = 6f;
    [SerializeField] private float difficultyEnergyMultiplier = 1f;

    [Header("Serviço atual")]
    [SerializeField] private float serviceReceivedValue = 35f;
    [SerializeField] private string currentServiceName = "Serviço padrão";

    [Header("Qualidade manual do jogador")]
    [SerializeField] private float playerSkillBonus = 0f;

    [Header("UI Opcional")]
    [SerializeField] private ServiceEvaluationUI serviceEvaluationUI;

    private ServiceSessionData currentSession;

    public bool HasActiveSession => currentSession != null;

    public void StartService(
        ClientNPC clientNPC,
        ClientRequestData requestData,
        float expectedDurationMinutes,
        float equipmentQuality,
        float productQuality,
        float comfortScore
    )
    {
        if (clientNPC == null)
            return;

        if (PlayerEnergySystem.Instance == null)
        {
            Debug.LogWarning("[BarberWorkController] PlayerEnergySystem.Instance não encontrado.");
            return;
        }

        currentSession = new ServiceSessionData();
        currentSession.clientNPC = clientNPC;
        currentSession.clientName = clientNPC.ClientDisplayName;
        currentSession.serviceStartGameMinutes = GameTimeSystem.Instance != null ? GameTimeSystem.Instance.TotalMinutesElapsed : 0f;

        ClientQueueData queueData = null;

        if (BarberQueueSystem.Instance != null)
            queueData = BarberQueueSystem.Instance.GetClientData(clientNPC);

        if (queueData != null)
        {
            currentSession.clientArrivalGameMinutes = queueData.arrivalGameMinutes;
            currentSession.waitingMinutes = queueData.GetWaitingMinutes(currentSession.serviceStartGameMinutes);
        }

        currentSession.expectedServiceDurationMinutes = Mathf.Max(1f, expectedDurationMinutes);
        currentSession.maxAcceptableServiceDurationMinutes = currentSession.expectedServiceDurationMinutes * 1.25f;
        currentSession.equipmentQualityScore = Mathf.Clamp(equipmentQuality, 0f, 5f);
        currentSession.productQualityScore = Mathf.Clamp(productQuality, 0f, 5f);
        currentSession.environmentComfortScore = Mathf.Clamp(comfortScore, 0f, 5f);
        currentSession.barberEnergyAtStart = PlayerEnergySystem.Instance.CurrentEnergy;

        // 🔥 NOVO: sistema de preço e satisfação
        if (requestData != null)
        {
            currentSession.finalChargedPrice = requestData.ServicePrice;

            if (GlobalGameplayManagement.Instance != null)
            {
                currentSession.suggestedPrice =
                    GlobalGameplayManagement.Instance.CalculateSuggestedPriceForRequest(requestData);

                currentSession.pricingSatisfactionScore =
                    GlobalGameplayManagement.Instance.GetPriceSatisfactionScore(
                        currentSession.finalChargedPrice,
                        currentSession.suggestedPrice
                    );
            }
            else
            {
                currentSession.suggestedPrice = currentSession.finalChargedPrice;
                currentSession.pricingSatisfactionScore = 1f;
            }
        }
    }

    public float GetAdjustedServiceDuration(float baseDurationMinutes)
    {
        baseDurationMinutes = Mathf.Max(1f, baseDurationMinutes);

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

    public ServiceEvaluationResult FinishService(
        float actualDurationMinutes,
        float equipmentQuality,
        float productQuality,
        bool hadMistakes
    )
    {
        if (currentSession == null)
        {
            Debug.LogWarning("[BarberWorkController] Não existe sessão de atendimento ativa.");
            return null;
        }

        if (PlayerEnergySystem.Instance == null)
        {
            Debug.LogWarning("[BarberWorkController] PlayerEnergySystem.Instance não encontrado.");
            currentSession = null;
            return null;
        }

        currentSession.serviceEndGameMinutes =
            GameTimeSystem.Instance != null ? GameTimeSystem.Instance.TotalMinutesElapsed : 0f;

        currentSession.actualServiceDurationMinutes = Mathf.Max(1f, actualDurationMinutes);
        currentSession.barberEnergyAtEnd = PlayerEnergySystem.Instance.CurrentEnergy;
        currentSession.hadMistakes = hadMistakes;
        currentSession.clientWasServed = true;
        currentSession.usedGoodProducts = productQuality >= 3.5f;
        currentSession.equipmentQualityScore = Mathf.Clamp(equipmentQuality, 0f, 5f);
        currentSession.productQualityScore = Mathf.Clamp(productQuality, 0f, 5f);
        currentSession.manualServiceQualityScore =
            CalculateFinalQualityScore(equipmentQuality, productQuality, hadMistakes);

        float energyCost =
            CalculateEnergyCost(currentSession.actualServiceDurationMinutes,
                                currentSession.expectedServiceDurationMinutes);

        // 🔥 NOVO: sistema de overwork (cansaço dinâmico)
        if (GlobalGameplayManagement.Instance != null)
        {
            currentSession.overworkMultiplier =
                GlobalGameplayManagement.Instance.GetOverworkEnergyMultiplier();

            energyCost *= currentSession.overworkMultiplier;
        }

        PlayerEnergySystem.Instance.ConsumeEnergy(energyCost);
        PlayerEnergySystem.Instance.AddFatigue(energyCost * 0.8f);

        ServiceEvaluationResult result = null;

        if (ClientEvaluationSystem.Instance != null)
        {
            result = ClientEvaluationSystem.Instance.EvaluateService(currentSession);
        }
        else
        {
            Debug.LogWarning("[BarberWorkController] ClientEvaluationSystem.Instance não encontrado.");
        }

        AddServiceHistory(result);

        if (serviceEvaluationUI != null && result != null)
            serviceEvaluationUI.Show(result);

        currentSession = null;

        return result;
    }

    public void SetCurrentServiceInfo(string serviceName, float receivedValue)
    {
        currentServiceName = string.IsNullOrWhiteSpace(serviceName) ? "Serviço" : serviceName;
        serviceReceivedValue = Mathf.Max(0f, receivedValue);
    }

    private float CalculateEnergyCost(float actualDurationMinutes, float expectedDurationMinutes)
    {
        float energyCost = baseEnergyCost * difficultyEnergyMultiplier;

        if (actualDurationMinutes > expectedDurationMinutes)
        {
            float overtime = actualDurationMinutes - expectedDurationMinutes;
            energyCost += overtime * 0.18f;
        }

        if (expectedDurationMinutes >= 25f)
            energyCost += 1.5f;

        return Mathf.Max(0f, energyCost);
    }

    private void AddServiceHistory(ServiceEvaluationResult result)
    {
        if (ServiceHistorySystem.Instance == null)
            return;

        if (GameTimeSystem.Instance == null)
            return;

        float finalScore = result != null ? result.finalScore : 0f;

        ServiceHistorySystem.Instance.AddEntry(
            day: GameTimeSystem.Instance.CurrentDay,
            timeOfDayMinutes: GameTimeSystem.Instance.CurrentTimeOfDayMinutes,
            clientName: currentSession.clientName,
            receivedValue: serviceReceivedValue,
            finalRating: finalScore,
            serviceName: currentServiceName
        );
    }

    private float CalculateFinalQualityScore(float equipmentQuality, float productQuality, bool hadMistakes)
    {
        float value = CalculateManualServiceQuality(equipmentQuality, productQuality);

        if (hadMistakes)
            value -= 1f;

        return Mathf.Clamp(value, 0f, 5f);
    }
}