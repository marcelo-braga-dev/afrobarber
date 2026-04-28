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
    [SerializeField] private float aestheticWeight = 0.5f;
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
        PersistentGameObject.MakePersistent(gameObject);
    }

    public ServiceEvaluationResult EvaluateService(ServiceSessionData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[ClientEvaluationSystem] ServiceSessionData nulo.");
            return null;
        }

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
        result.aestheticScore = Mathf.Clamp(data.environmentAestheticScore, 0f, 5f);
        result.pricingScore = Mathf.Clamp(data.pricingSatisfactionScore * 5f, 0f, 5f);

        if (randomizeIndividualFactors)
        {
            result.waitingScore = ApplyRandomMargin(result.waitingScore);
            result.serviceQualityScore = ApplyRandomMargin(result.serviceQualityScore);
            result.serviceTimeScore = ApplyRandomMargin(result.serviceTimeScore);
            result.barberConditionScore = ApplyRandomMargin(result.barberConditionScore);
            result.equipmentScore = ApplyRandomMargin(result.equipmentScore);
            result.comfortScore = ApplyRandomMargin(result.comfortScore);
            result.aestheticScore = ApplyRandomMargin(result.aestheticScore);
            result.pricingScore = ApplyRandomMargin(result.pricingScore);
        }

        result.attendanceScore = CalculateWeightedAverage(
            result.serviceQualityScore, 2f,
            result.serviceTimeScore, 1.5f,
            result.barberConditionScore, 1f
        );

        result.structureScore = CalculateWeightedAverage(
            result.equipmentScore, 1.2f,
            result.comfortScore, 1f,
            result.aestheticScore, 1f
        );

        result.experienceScore = CalculateWeightedAverage(
            result.waitingScore, 1f,
            result.comfortScore, 1f,
            result.aestheticScore, 0.8f,
            result.pricingScore, 1f
        );

        result.waitingComment = GetWaitingComment(result.waitingScore);
        result.qualityComment = GetQualityComment(result.serviceQualityScore);
        result.timeComment = GetServiceTimeComment(result.serviceTimeScore);
        result.barberConditionComment = GetBarberConditionComment(result.barberConditionScore);
        result.equipmentComment = GetEquipmentComment(result.equipmentScore);
        result.comfortComment = GetComfortComment(result.comfortScore);
        result.aestheticComment = GetAestheticComment(result.aestheticScore);
        result.pricingComment = GetPricingComment(result.pricingScore);

        result.attendanceComment = GetAttendanceComment(result.attendanceScore);
        result.structureComment = GetStructureComment(result.structureScore);
        result.experienceComment = GetExperienceComment(result.experienceScore);

        float baseFinalScore = CalculateWeightedAverage(
            result.waitingScore, waitingWeight,
            result.serviceQualityScore, serviceQualityWeight,
            result.serviceTimeScore, serviceTimeWeight,
            result.barberConditionScore, barberConditionWeight,
            result.equipmentScore, equipmentWeight,
            result.comfortScore, comfortWeight,
            result.aestheticScore, aestheticWeight,
            result.pricingScore, pricingWeight
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

        if (BarbershopRatingManager.Instance != null)
            BarbershopRatingManager.Instance.AddReview(result);

        Debug.Log(
            $"[ClientEvaluationSystem] Nota final: {result.finalScore} | " +
            $"Atendimento={result.attendanceScore:0.0}, Estrutura={result.structureScore:0.0}, Experiência={result.experienceScore:0.0} | " +
            $"Espera={result.waitingScore:0.0}, Qualidade={result.serviceQualityScore:0.0}, Tempo={result.serviceTimeScore:0.0}, " +
            $"Barbeiro={result.barberConditionScore:0.0}, Equipamentos={result.equipmentScore:0.0}, " +
            $"Conforto={result.comfortScore:0.0}, Estética={result.aestheticScore:0.0}, Preço={result.pricingScore:0.0}"
        );

        return result;
    }

    private float CalculateWeightedAverage(
        float a, float weightA,
        float b, float weightB,
        float c, float weightC)
    {
        float totalWeight = weightA + weightB + weightC;

        if (totalWeight <= 0f)
            return 0f;

        float weightedSum =
            (a * weightA) +
            (b * weightB) +
            (c * weightC);

        return Mathf.Clamp(weightedSum / totalWeight, 0f, 5f);
    }

    private float CalculateWeightedAverage(
        float a, float weightA,
        float b, float weightB,
        float c, float weightC,
        float d, float weightD)
    {
        float totalWeight = weightA + weightB + weightC + weightD;

        if (totalWeight <= 0f)
            return 0f;

        float weightedSum =
            (a * weightA) +
            (b * weightB) +
            (c * weightC) +
            (d * weightD);

        return Mathf.Clamp(weightedSum / totalWeight, 0f, 5f);
    }

    private float CalculateWeightedAverage(
        float a, float weightA,
        float b, float weightB,
        float c, float weightC,
        float d, float weightD,
        float e, float weightE,
        float f, float weightF,
        float g, float weightG,
        float h, float weightH)
    {
        float totalWeight = weightA + weightB + weightC + weightD + weightE + weightF + weightG + weightH;

        if (totalWeight <= 0f)
            return 0f;

        float weightedSum =
            (a * weightA) +
            (b * weightB) +
            (c * weightC) +
            (d * weightD) +
            (e * weightE) +
            (f * weightF) +
            (g * weightG) +
            (h * weightH);

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
        float start = Mathf.Clamp01(barberEnergyAtStart > 1f ? barberEnergyAtStart / 100f : barberEnergyAtStart);
        float end = Mathf.Clamp01(barberEnergyAtEnd > 1f ? barberEnergyAtEnd / 100f : barberEnergyAtEnd);

        float averageEnergy = (start + end) * 0.5f;
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

    private string GetAestheticComment(float score) => GetCommentByScore(
        score,
        "O visual da barbearia prejudicou a experiência.",
        "Ambiente visualmente simples, pode melhorar.",
        "Barbearia bonita e agradável.",
        "Visual marcante, estiloso e muito atrativo."
    );

    private string GetPricingComment(float score) => GetCommentByScore(
        score,
        "O preço cobrado pareceu injusto.",
        "O preço ficou um pouco acima do esperado.",
        "Preço aceitável pelo serviço.",
        "Preço justo pela experiência entregue."
    );

    private string GetAttendanceComment(float score) => GetCommentByScore(
        score,
        "Atendimento precisa melhorar bastante.",
        "Atendimento razoável, com pontos de atenção.",
        "Bom atendimento.",
        "Atendimento excelente."
    );

    private string GetStructureComment(float score) => GetCommentByScore(
        score,
        "Estrutura da barbearia prejudicou a experiência.",
        "Estrutura simples, pode melhorar.",
        "Boa estrutura.",
        "Estrutura excelente, confortável e estilosa."
    );

    private string GetExperienceComment(float score) => GetCommentByScore(
        score,
        "Experiência geral desconfortável.",
        "Experiência razoável.",
        "Boa experiência geral.",
        "Experiência muito agradável e memorável."
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