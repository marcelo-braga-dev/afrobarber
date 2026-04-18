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

    [Header("Detalhes")]
    [SerializeField] private TMP_Text waitingScoreText;
    [SerializeField] private TMP_Text qualityScoreText;
    [SerializeField] private TMP_Text timeScoreText;
    [SerializeField] private TMP_Text barberConditionScoreText;
    [SerializeField] private TMP_Text equipmentScoreText;
    [SerializeField] private TMP_Text comfortScoreText;

    [Header("Comentários")]
    [SerializeField] private TMP_Text waitingCommentText;
    [SerializeField] private TMP_Text qualityCommentText;
    [SerializeField] private TMP_Text timeCommentText;
    [SerializeField] private TMP_Text barberConditionCommentText;
    [SerializeField] private TMP_Text equipmentCommentText;
    [SerializeField] private TMP_Text comfortCommentText;

    private readonly CultureInfo brazilCulture = new CultureInfo("pt-BR");

    private void Start()
    {
        if (finalScoreSlider != null)
        {
            finalScoreSlider.minValue = 0f;
            finalScoreSlider.maxValue = 5f;
        }

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void Show(ServiceEvaluationResult result)
    {
        if (result == null)
            return;

        if (panel != null)
            panel.SetActive(true);

        SetText(finalScoreText, $"Nota Final: {Format(result.finalScore)}/5");
        SetText(finalCommentText, result.finalComment);

        if (finalScoreSlider != null)
            finalScoreSlider.value = result.finalScore;

        SetText(waitingScoreText, $"Espera: {Format(result.waitingScore)}/5");
        SetText(qualityScoreText, $"Qualidade: {Format(result.serviceQualityScore)}/5");
        SetText(timeScoreText, $"Tempo do Serviço: {Format(result.serviceTimeScore)}/5");
        SetText(barberConditionScoreText, $"Condição do Barbeiro: {Format(result.barberConditionScore)}/5");
        SetText(equipmentScoreText, $"Equipamentos: {Format(result.equipmentScore)}/5");
        SetText(comfortScoreText, $"Experiência: {Format(result.comfortScore)}/5");

        SetText(waitingCommentText, result.waitingComment);
        SetText(qualityCommentText, result.qualityComment);
        SetText(timeCommentText, result.timeComment);
        SetText(barberConditionCommentText, result.barberConditionComment);
        SetText(equipmentCommentText, result.equipmentComment);
        SetText(comfortCommentText, result.comfortComment);
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private string Format(float value)
    {
        return value.ToString("0.0", brazilCulture);
    }

    private void SetText(TMP_Text target, string value)
    {
        if (target != null)
            target.text = value;
    }
}