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

    [Header("Notas")]
    [SerializeField] private Slider waitingScoreSlider;
    [SerializeField] private Slider qualityScoreSlider;
    [SerializeField] private Slider timeScoreSlider;
    [SerializeField] private Slider barberConditionScoreSlider;
    [SerializeField] private Slider equipmentScoreSlider;
    [SerializeField] private Slider comfortScoreSlider;

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
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
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