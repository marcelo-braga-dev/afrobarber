using UnityEngine;

public class ClientEvaluationSystem : MonoBehaviour
{
    public static ClientEvaluationSystem Instance { get; private set; }

    [Header("Pesos da avaliação")]
    [SerializeField] private float waitingWeight = 1f;
    [SerializeField] private float serviceQualityWeight = 2f;
    [SerializeField] private float serviceTimeWeight = 1.5f;
    [SerializeField] private float barberConditionWeight = 1f;
    [SerializeField] private float equipmentWeight = 1f;
    [SerializeField] private float comfortWeight = 0.5f;

    [Header("Regras")]
    [SerializeField] private float defaultMaxWaitingMinutes = 90f;
    [SerializeField] private float defaultMaxServiceDelayPercent = 0.35f;

    [Header("Aleatoriedade")]
    [Tooltip("Margem percentual máxima de variação. Ex.: 0.20 = 20%")]
    [Range(0f, 1f)]
    [SerializeField] private float randomMarginPercent = 0.20f;

    [Tooltip("Aplica variação aleatória em cada fator individual.")]
    [SerializeField] private bool randomizeIndividualFactors = true;

    [Tooltip("Aplica variação aleatória na nota final.")]
    [SerializeField] private bool randomizeFinalScore = true;

    [Tooltip("Impede que notas finais seguidas saiam exatamente iguais.")]
    [SerializeField] private bool avoidRepeatedFinalScore = true;

    private float lastFinalScore = -1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public ServiceEvaluationResult EvaluateService(ServiceSessionData data)
    {
        ServiceEvaluationResult result = new ServiceEvaluationResult();

        float waitingLimit = Mathf.Max(1f, defaultMaxWaitingMinutes);
        result.waitingScore = EvaluateWaitingScore(data.waitingMinutes, waitingLimit);

        result.serviceQualityScore = Mathf.Clamp(data.manualServiceQualityScore, 0f, 5f);

        float serviceLimit = Mathf.Max(
            data.expectedServiceDurationMinutes,
            data.maxAcceptableServiceDurationMinutes > 0f
                ? data.maxAcceptableServiceDurationMinutes
                : data.expectedServiceDurationMinutes * (1f + defaultMaxServiceDelayPercent)
        );

        result.serviceTimeScore = EvaluateServiceTimeScore(data.actualServiceDurationMinutes, serviceLimit);
        result.barberConditionScore = EvaluateBarberConditionScore(data.barberEnergyAtStart, data.barberEnergyAtEnd);
        result.equipmentScore = EvaluateEquipmentScore(data.GetAverageToolQuality());
        result.comfortScore = Mathf.Clamp(data.environmentComfortScore, 0f, 5f);

        if (randomizeIndividualFactors)
        {
            result.waitingScore = ApplyRandomMargin(result.waitingScore);
            result.serviceQualityScore = ApplyRandomMargin(result.serviceQualityScore);
            result.serviceTimeScore = ApplyRandomMargin(result.serviceTimeScore);
            result.barberConditionScore = ApplyRandomMargin(result.barberConditionScore);
            result.equipmentScore = ApplyRandomMargin(result.equipmentScore);
            result.comfortScore = ApplyRandomMargin(result.comfortScore);
        }

        result.waitingComment = GetWaitingComment(result.waitingScore);
        result.qualityComment = GetQualityComment(result.serviceQualityScore);
        result.timeComment = GetServiceTimeComment(result.serviceTimeScore);
        result.barberConditionComment = GetBarberConditionComment(result.barberConditionScore);
        result.equipmentComment = GetEquipmentComment(result.equipmentScore);
        result.comfortComment = GetComfortComment(result.comfortScore);

        float baseFinalScore = CalculateWeightedAverage(
            result.waitingScore, waitingWeight,
            result.serviceQualityScore, serviceQualityWeight,
            result.serviceTimeScore, serviceTimeWeight,
            result.barberConditionScore, barberConditionWeight,
            result.equipmentScore, equipmentWeight,
            result.comfortScore, comfortWeight
        );

        if (randomizeFinalScore)
        {
            baseFinalScore = ApplyRandomMargin(baseFinalScore);
        }

        baseFinalScore = Mathf.Clamp(baseFinalScore, 0f, 5f);
        baseFinalScore = Mathf.Round(baseFinalScore * 10f) / 10f;

        if (avoidRepeatedFinalScore)
        {
            baseFinalScore = EnsureDifferentFromLast(baseFinalScore);
        }

        result.finalScore = baseFinalScore;
        result.finalComment = GetFinalComment(result.finalScore);

        lastFinalScore = result.finalScore;

        Debug.Log(
            $"[ClientEvaluationSystem] Nota final: {result.finalScore} | " +
            $"Espera={result.waitingScore}, Qualidade={result.serviceQualityScore}, Tempo={result.serviceTimeScore}, " +
            $"Barbeiro={result.barberConditionScore}, Equipamentos={result.equipmentScore}, Conforto={result.comfortScore}"
        );

        return result;
    }

    private float EvaluateWaitingScore(float waitingMinutes, float maxAcceptableWaiting)
    {
        if (waitingMinutes <= 5f)
            return 5f;

        float ratio = waitingMinutes / maxAcceptableWaiting;

        if (ratio <= 0.20f) return 4.7f;
        if (ratio <= 0.35f) return 4.2f;
        if (ratio <= 0.50f) return 3.6f;
        if (ratio <= 0.70f) return 2.8f;
        if (ratio <= 0.90f) return 2f;
        if (ratio <= 1.10f) return 1.2f;

        return 0.5f;
    }

    private float EvaluateServiceTimeScore(float actualDuration, float maxAcceptableDuration)
    {
        if (actualDuration <= maxAcceptableDuration * 0.85f) return 5f;
        if (actualDuration <= maxAcceptableDuration) return 4.4f;
        if (actualDuration <= maxAcceptableDuration * 1.15f) return 3.6f;
        if (actualDuration <= maxAcceptableDuration * 1.30f) return 2.6f;
        if (actualDuration <= maxAcceptableDuration * 1.50f) return 1.5f;

        return 0.7f;
    }

    private float EvaluateBarberConditionScore(float energyAtStart, float energyAtEnd)
    {
        float avgEnergy = (energyAtStart + energyAtEnd) * 0.5f;
        float normalized = Mathf.Clamp01(avgEnergy / 100f);

        if (normalized >= 0.85f) return 5f;
        if (normalized >= 0.65f) return 4.4f;
        if (normalized >= 0.45f) return 3.5f;
        if (normalized >= 0.25f) return 2.4f;
        if (normalized >= 0.10f) return 1.4f;

        return 0.6f;
    }

    private float EvaluateEquipmentScore(float averageToolQuality)
    {
        return Mathf.Clamp(averageToolQuality, 0f, 5f);
    }

    private float CalculateWeightedAverage(
        float a, float weightA,
        float b, float weightB,
        float c, float weightC,
        float d, float weightD,
        float e, float weightE,
        float f, float weightF)
    {
        float totalWeight = weightA + weightB + weightC + weightD + weightE + weightF;

        if (totalWeight <= 0f)
            return 0f;

        float weightedSum =
            (a * weightA) +
            (b * weightB) +
            (c * weightC) +
            (d * weightD) +
            (e * weightE) +
            (f * weightF);

        return Mathf.Clamp(weightedSum / totalWeight, 0f, 5f);
    }

    private float ApplyRandomMargin(float baseValue)
    {
        if (randomMarginPercent <= 0f)
            return Mathf.Clamp(baseValue, 0f, 5f);

        float variation = baseValue * randomMarginPercent;
        float min = baseValue - variation;
        float max = baseValue + variation;

        float randomized = Random.Range(min, max);
        return Mathf.Clamp(randomized, 0f, 5f);
    }

    private float EnsureDifferentFromLast(float value)
    {
        if (lastFinalScore < 0f)
            return value;

        if (!Mathf.Approximately(value, lastFinalScore))
            return value;

        float offset = Random.value < 0.5f ? -0.1f : 0.1f;
        value += offset;
        value = Mathf.Clamp(value, 0f, 5f);
        value = Mathf.Round(value * 10f) / 10f;

        return value;
    }

    private string GetWaitingComment(float score)
    {
        if (score >= 4.5f) return "Espera excelente";
        if (score >= 3.5f) return "Espera aceitável";
        if (score >= 2.5f) return "Demorou um pouco";
        if (score >= 1.5f) return "Esperou demais";
        return "Tempo de espera muito ruim";
    }

    private string GetQualityComment(float score)
    {
        if (score >= 4.5f) return "Serviço excelente";
        if (score >= 3.5f) return "Serviço bom";
        if (score >= 2.5f) return "Serviço mediano";
        if (score >= 1.5f) return "Qualidade abaixo do esperado";
        return "Serviço ruim";
    }

    private string GetServiceTimeComment(float score)
    {
        if (score >= 4.5f) return "Atendimento rápido";
        if (score >= 3.5f) return "Tempo adequado";
        if (score >= 2.5f) return "Atendimento um pouco demorado";
        if (score >= 1.5f) return "Atendimento lento";
        return "Atendimento muito demorado";
    }

    private string GetBarberConditionComment(float score)
    {
        if (score >= 4.5f) return "Barbeiro em ótima condição";
        if (score >= 3.5f) return "Bom ritmo de trabalho";
        if (score >= 2.5f) return "Cansaço perceptível";
        if (score >= 1.5f) return "Muito cansado";
        return "Exaustão afetou o atendimento";
    }

    private string GetEquipmentComment(float score)
    {
        if (score >= 4.5f) return "Equipamentos excelentes";
        if (score >= 3.5f) return "Equipamentos bons";
        if (score >= 2.5f) return "Equipamentos medianos";
        if (score >= 1.5f) return "Equipamentos fracos";
        return "Equipamentos comprometeram o serviço";
    }

    private string GetComfortComment(float score)
    {
        if (score >= 4.5f) return "Experiência muito agradável";
        if (score >= 3.5f) return "Experiência boa";
        if (score >= 2.5f) return "Experiência normal";
        if (score >= 1.5f) return "Experiência desconfortável";
        return "Experiência ruim";
    }

    private string GetFinalComment(float score)
    {
        if (score >= 4.5f) return "Cliente saiu extremamente satisfeito";
        if (score >= 3.5f) return "Cliente saiu satisfeito";
        if (score >= 2.5f) return "Cliente achou o atendimento razoável";
        if (score >= 1.5f) return "Cliente ficou insatisfeito";
        return "Cliente saiu muito insatisfeito";
    }
}