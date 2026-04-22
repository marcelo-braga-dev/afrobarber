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
    [SerializeField] private float pricingWeight = 1.5f;

    [Header("Regras")]
    [SerializeField] private float defaultMaxWaitingMinutes = 90f;
    [SerializeField] private float defaultMaxServiceDelayPercent = 0.35f;

    [Header("Aleatoriedade")]
    [Range(0f, 1f)]
    [SerializeField] private float randomMarginPercent = 0.20f;
    [SerializeField] private bool randomizeIndividualFactors = true;
    [SerializeField] private bool randomizeFinalScore = true;
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

        float pricingScore = Mathf.Clamp(data.pricingSatisfactionScore * 5f, 0f, 5f);

        if (randomizeIndividualFactors)
        {
            result.waitingScore = ApplyRandomMargin(result.waitingScore);
            result.serviceQualityScore = ApplyRandomMargin(result.serviceQualityScore);
            result.serviceTimeScore = ApplyRandomMargin(result.serviceTimeScore);
            result.barberConditionScore = ApplyRandomMargin(result.barberConditionScore);
            result.equipmentScore = ApplyRandomMargin(result.equipmentScore);
            result.comfortScore = ApplyRandomMargin(result.comfortScore);
            pricingScore = ApplyRandomMargin(pricingScore);
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
            result.comfortScore, comfortWeight,
            pricingScore, pricingWeight
        );

        if (randomizeFinalScore)
            baseFinalScore = ApplyRandomMargin(baseFinalScore);

        baseFinalScore = Mathf.Clamp(baseFinalScore, 0f, 5f);
        baseFinalScore = Mathf.Round(baseFinalScore * 10f) / 10f;

        if (avoidRepeatedFinalScore)
            baseFinalScore = EnsureDifferentFromLast(baseFinalScore);

        result.finalScore = baseFinalScore;
        result.finalComment = GetFinalComment(result.finalScore);

        lastFinalScore = result.finalScore;

        Debug.Log(
            $"[ClientEvaluationSystem] Nota final: {result.finalScore} | " +
            $"Espera={result.waitingScore}, Qualidade={result.serviceQualityScore}, Tempo={result.serviceTimeScore}, " +
            $"Barbeiro={result.barberConditionScore}, Equipamentos={result.equipmentScore}, Conforto={result.comfortScore}, Preço={pricingScore}"
        );

        return result;
    }

    private float CalculateWeightedAverage(
        float a, float weightA,
        float b, float weightB,
        float c, float weightC,
        float d, float weightD,
        float e, float weightE,
        float f, float weightF,
        float g, float weightG)
    {
        float totalWeight = weightA + weightB + weightC + weightD + weightE + weightF + weightG;

        if (totalWeight <= 0f)
            return 0f;

        float weightedSum =
            (a * weightA) +
            (b * weightB) +
            (c * weightC) +
            (d * weightD) +
            (e * weightE) +
            (f * weightF) +
            (g * weightG);

        return Mathf.Clamp(weightedSum / totalWeight, 0f, 5f);
    }

    private float EvaluateWaitingScore(float waitingMinutes, float waitingLimitMinutes)
    {
        waitingMinutes = Mathf.Max(0f, waitingMinutes);
        waitingLimitMinutes = Mathf.Max(1f, waitingLimitMinutes);

        if (waitingMinutes <= 0f)
            return 5f;

        float normalized = Mathf.Clamp01(waitingMinutes / waitingLimitMinutes);
        return Mathf.Clamp(5f - (normalized * 5f), 0f, 5f);
    }

    private float EvaluateServiceTimeScore(float actualDurationMinutes, float serviceLimitMinutes)
    {
        actualDurationMinutes = Mathf.Max(0f, actualDurationMinutes);
        serviceLimitMinutes = Mathf.Max(1f, serviceLimitMinutes);

        float normalized = Mathf.Clamp01(actualDurationMinutes / serviceLimitMinutes);
        return Mathf.Clamp(5f - (normalized * 5f), 0f, 5f);
    }

    private float EvaluateBarberConditionScore(float barberEnergyAtStart, float barberEnergyAtEnd)
    {
        float normalizedStart = Mathf.Clamp01(barberEnergyAtStart);
        float normalizedEnd = Mathf.Clamp01(barberEnergyAtEnd);
        float averageEnergy = (normalizedStart + normalizedEnd) * 0.5f;
        return Mathf.Clamp(averageEnergy * 5f, 0f, 5f);
    }

    private float EvaluateEquipmentScore(float equipmentQuality)
    {
        return Mathf.Clamp(equipmentQuality, 0f, 5f);
    }

    private float ApplyRandomMargin(float value)
    {
        if (randomMarginPercent <= 0f)
            return Mathf.Clamp(value, 0f, 5f);

        float margin = Mathf.Abs(value) * randomMarginPercent;
        if (margin < 0.05f)
            margin = 0.05f;

        float randomized = value + Random.Range(-margin, margin);
        return Mathf.Clamp(randomized, 0f, 5f);
    }

    private float EnsureDifferentFromLast(float score)
    {
        if (lastFinalScore < 0f || !Mathf.Approximately(score, lastFinalScore))
            return score;

        float adjusted = score + Random.Range(-0.2f, 0.2f);
        adjusted = Mathf.Clamp(adjusted, 0f, 5f);
        return Mathf.Round(adjusted * 10f) / 10f;
    }

    private string GetWaitingComment(float score) => GetCommentByScore(
        score,
        "Demora excessiva para começar o atendimento.",
        "Tempo de espera um pouco alto.",
        "Espera aceitável.",
        "Atendimento começou rapidamente."
    );

    private string GetQualityComment(float score) => GetCommentByScore(
        score,
        "O resultado final ficou abaixo do esperado.",
        "Qualidade razoável, mas pode melhorar.",
        "Bom trabalho no serviço realizado.",
        "Corte excelente, superou as expectativas."
    );

    private string GetServiceTimeComment(float score) => GetCommentByScore(
        score,
        "Serviço demorou mais do que o aceitável.",
        "Tempo de execução poderia ser menor.",
        "Tempo de atendimento adequado.",
        "Serviço ágil e bem executado."
    );

    private string GetBarberConditionComment(float score) => GetCommentByScore(
        score,
        "O barbeiro parecia muito cansado durante o serviço.",
        "Percebi um pouco de desgaste no atendimento.",
        "Atendimento estável, sem grandes oscilações.",
        "Barbeiro muito disposto e focado."
    );

    private string GetEquipmentComment(float score) => GetCommentByScore(
        score,
        "Ferramentas e produtos impactaram negativamente a experiência.",
        "Equipamentos razoáveis, mas com espaço para melhoria.",
        "Equipamentos em boas condições.",
        "Ferramentas e produtos de ótima qualidade."
    );

    private string GetComfortComment(float score) => GetCommentByScore(
        score,
        "Ambiente desconfortável durante o atendimento.",
        "Conforto mediano no espaço.",
        "Ambiente confortável.",
        "Ambiente muito agradável e acolhedor."
    );

    private string GetFinalComment(float finalScore) => GetCommentByScore(
        finalScore,
        "Experiência ruim. Precisa melhorar bastante.",
        "Experiência razoável, com pontos importantes para ajustar.",
        "Boa experiência geral.",
        "Excelente experiência. Voltarei com certeza."
    );

    private string GetCommentByScore(
        float score,
        string lowComment,
        string mediumComment,
        string goodComment,
        string greatComment)
    {
        if (score < 2f) return lowComment;
        if (score < 3f) return mediumComment;
        if (score < 4.5f) return goodComment;
        return greatComment;
    }
}