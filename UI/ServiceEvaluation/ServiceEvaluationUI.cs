using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServiceEvaluationUI : MonoBehaviour
{
    [Header("Painel")]
    [SerializeField] private GameObject panel;

    [Header("Resumo")]
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text finalCommentText;
    [SerializeField] private Slider finalScoreSlider;

    [Header("Notas antigas")]
    [SerializeField] private Slider waitingScoreSlider;
    [SerializeField] private Slider qualityScoreSlider;
    [SerializeField] private Slider timeScoreSlider;
    [SerializeField] private Slider barberConditionScoreSlider;
    [SerializeField] private Slider equipmentScoreSlider;
    [SerializeField] private Slider comfortScoreSlider;

    [Header("Notas novas opcionais")]
    [SerializeField] private Slider aestheticScoreSlider;
    [SerializeField] private Slider pricingScoreSlider;

    [Header("Notas agrupadas opcionais")]
    [SerializeField] private TMP_Text attendanceScoreText;
    [SerializeField] private Slider attendanceScoreSlider;

    [SerializeField] private TMP_Text structureScoreText;
    [SerializeField] private Slider structureScoreSlider;

    [SerializeField] private TMP_Text experienceScoreText;
    [SerializeField] private Slider experienceScoreSlider;

    [Header("Comentários agrupados opcionais")]
    [SerializeField] private TMP_Text attendanceCommentText;
    [SerializeField] private TMP_Text structureCommentText;
    [SerializeField] private TMP_Text experienceCommentText;

    [Header("Configuração")]
    [SerializeField] private bool enableDebugLogs = true;

    private readonly CultureInfo brazilCulture = new CultureInfo("pt-BR");

    private void Start()
    {
        ConfigureSlider(finalScoreSlider);
        ConfigureSlider(waitingScoreSlider);
        ConfigureSlider(qualityScoreSlider);
        ConfigureSlider(timeScoreSlider);
        ConfigureSlider(barberConditionScoreSlider);
        ConfigureSlider(equipmentScoreSlider);
        ConfigureSlider(comfortScoreSlider);
        ConfigureSlider(aestheticScoreSlider);
        ConfigureSlider(pricingScoreSlider);
        ConfigureSlider(attendanceScoreSlider);
        ConfigureSlider(structureScoreSlider);
        ConfigureSlider(experienceScoreSlider);

        if (panel != null)
            panel.SetActive(false);
    }

    public void Show(ServiceEvaluationResult result)
    {
        if (result == null)
            return;

        if (panel != null)
            panel.SetActive(true);

        SetText(finalScoreText, $"{Format(result.finalScore)}/5");
        SetText(finalCommentText, result.finalComment);

        SetSlider(finalScoreSlider, result.finalScore);
        SetSlider(waitingScoreSlider, result.waitingScore);
        SetSlider(qualityScoreSlider, result.serviceQualityScore);
        SetSlider(timeScoreSlider, result.serviceTimeScore);
        SetSlider(barberConditionScoreSlider, result.barberConditionScore);
        SetSlider(equipmentScoreSlider, result.equipmentScore);
        SetSlider(comfortScoreSlider, result.comfortScore);
        SetSlider(aestheticScoreSlider, result.aestheticScore);
        SetSlider(pricingScoreSlider, result.pricingScore);

        SetText(attendanceScoreText, $"{Format(result.attendanceScore)}/5");
        SetText(structureScoreText, $"{Format(result.structureScore)}/5");
        SetText(experienceScoreText, $"{Format(result.experienceScore)}/5");

        SetSlider(attendanceScoreSlider, result.attendanceScore);
        SetSlider(structureScoreSlider, result.structureScore);
        SetSlider(experienceScoreSlider, result.experienceScore);

        SetText(attendanceCommentText, result.attendanceComment);
        SetText(structureCommentText, result.structureComment);
        SetText(experienceCommentText, result.experienceComment);
    }

    public void ShowAdvanced(AdvancedServiceResult result)
    {
        if (result == null)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[ServiceEvaluationUI] AdvancedServiceResult nulo. Não foi possível mostrar avaliação avançada.");

            return;
        }

        if (panel != null)
            panel.SetActive(true);

        float finalScore = Mathf.Clamp(result.finalScore, 0f, 5f);
        float timeScore = CalculateAdvancedTimeScore(result);
        float qualityScore = finalScore;
        float waitingScore = 5f;
        float barberConditionScore = finalScore;
        float equipmentScore = finalScore;
        float comfortScore = 4f;
        float aestheticScore = 4f;
        float pricingScore = 4f;

        float attendanceScore = CalculateAverage(qualityScore, timeScore, barberConditionScore);
        float structureScore = CalculateAverage(equipmentScore, comfortScore, aestheticScore);
        float experienceScore = CalculateAverage(waitingScore, comfortScore, aestheticScore, pricingScore);

        SetText(finalScoreText, $"{Format(finalScore)}/5");
        SetText(finalCommentText, BuildAdvancedFinalComment(result));

        SetSlider(finalScoreSlider, finalScore);
        SetSlider(waitingScoreSlider, waitingScore);
        SetSlider(qualityScoreSlider, qualityScore);
        SetSlider(timeScoreSlider, timeScore);
        SetSlider(barberConditionScoreSlider, barberConditionScore);
        SetSlider(equipmentScoreSlider, equipmentScore);
        SetSlider(comfortScoreSlider, comfortScore);
        SetSlider(aestheticScoreSlider, aestheticScore);
        SetSlider(pricingScoreSlider, pricingScore);

        SetText(attendanceScoreText, $"{Format(attendanceScore)}/5");
        SetText(structureScoreText, $"{Format(structureScore)}/5");
        SetText(experienceScoreText, $"{Format(experienceScore)}/5");

        SetSlider(attendanceScoreSlider, attendanceScore);
        SetSlider(structureScoreSlider, structureScore);
        SetSlider(experienceScoreSlider, experienceScore);

        SetText(attendanceCommentText, "Resumo do atendimento avançado.");
        SetText(structureCommentText, "Estrutura considerada adequada para o serviço.");
        SetText(experienceCommentText, "Experiência calculada pelo fluxo avançado.");

        if (enableDebugLogs)
        {
            Debug.Log(
                $"[ServiceEvaluationUI] Avaliação avançada exibida | " +
                $"Nota: {finalScore:0.0}/5 | Resultado: {result.finalRating}"
            );
        }
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private float CalculateAdvancedTimeScore(AdvancedServiceResult result)
    {
        if (result == null)
            return 3f;

        float expected = Mathf.Max(0.1f, result.expectedClientMinutes);
        float actual = Mathf.Max(0.1f, result.actualTotalMinutes);
        float difference = Mathf.Abs(actual - expected);
        float percentage = difference / expected;

        if (percentage <= 0.10f)
            return 5f;

        if (percentage <= 0.25f)
            return 4f;

        if (percentage <= 0.40f)
            return 3f;

        if (percentage <= 0.60f)
            return 2f;

        return 1f;
    }

    private string BuildAdvancedFinalComment(AdvancedServiceResult result)
    {
        if (result == null)
            return "Atendimento finalizado.";

        string ratingText = ConvertFinalRatingToText(result.finalRating);
        string timeText = BuildTimeComment(result);

        return $"Avaliação do cliente: {ratingText}. {timeText}";
    }

    private string BuildTimeComment(AdvancedServiceResult result)
    {
        float expected = Mathf.Max(0.1f, result.expectedClientMinutes);
        float actual = Mathf.Max(0.1f, result.actualTotalMinutes);

        if (actual <= expected * 0.9f)
            return "O atendimento foi rápido.";

        if (actual <= expected * 1.15f)
            return "O tempo do atendimento ficou dentro do esperado.";

        if (actual <= expected * 1.4f)
            return "O atendimento demorou um pouco mais que o esperado.";

        return "O atendimento demorou bastante e isso afetou a experiência.";
    }

    private string ConvertFinalRatingToText(ServiceFinalRating rating)
    {
        return rating switch
        {
            ServiceFinalRating.Horrivel => "horrível",
            ServiceFinalRating.Ruim => "ruim",
            ServiceFinalRating.MaisOuMenos => "mais ou menos",
            ServiceFinalRating.Bom => "bom",
            ServiceFinalRating.Maravilhoso => "maravilhoso",
            ServiceFinalRating.Perfeito => "perfeito",
            _ => "indefinida"
        };
    }

    private float CalculateAverage(params float[] values)
    {
        if (values == null || values.Length == 0)
            return 0f;

        float total = 0f;

        foreach (float value in values)
            total += Mathf.Clamp(value, 0f, 5f);

        return Mathf.Clamp(total / values.Length, 0f, 5f);
    }

    private void ConfigureSlider(Slider slider)
    {
        if (slider == null)
            return;

        slider.minValue = 0f;
        slider.maxValue = 5f;
        slider.wholeNumbers = false;
        slider.interactable = false;
    }

    private void SetSlider(Slider slider, float value)
    {
        if (slider == null)
            return;

        slider.value = Mathf.Clamp(value, 0f, 5f);
    }

    private string Format(float value)
    {
        return value.ToString("0.#", brazilCulture);
    }

    private void SetText(TMP_Text target, string value)
    {
        if (target != null)
            target.text = value;
    }
}