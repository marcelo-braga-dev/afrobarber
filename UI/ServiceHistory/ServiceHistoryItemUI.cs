using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServiceHistoryItemUI : MonoBehaviour
{
    [Header("Textos")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text hourText;
    [SerializeField] private TMP_Text clientNameText;
    [SerializeField] private TMP_Text receivedValueText;
    [SerializeField] private TMP_Text finalRatingText;
    [SerializeField] private TMP_Text serviceNameText;

    [Header("Avaliação")]
    [SerializeField] private Slider ratingSlider;
    [SerializeField] private Image[] starFillImages = new Image[5];

    private readonly CultureInfo brazilCulture = new CultureInfo("pt-BR");

    private void Awake()
    {
        SetupSlider();
        SetupStars();
    }

    public void Setup(ServiceHistoryEntry entry)
    {
        if (entry == null)
            return;

        if (dayText != null)
            dayText.text = $"Dia {entry.day}";

        if (hourText != null)
            hourText.text = FormatHour(entry.timeOfDayMinutes);

        if (clientNameText != null)
            clientNameText.text = entry.clientName;

        if (receivedValueText != null)
            receivedValueText.text = $"R$ {entry.receivedValue.ToString("0.00", brazilCulture)}";

        if (finalRatingText != null)
            finalRatingText.text = $"{entry.finalRating.ToString("0.0", brazilCulture)}/5";

        if (serviceNameText != null)
            serviceNameText.text = string.IsNullOrWhiteSpace(entry.serviceName) ? "Serviço padrão" : entry.serviceName;

        if (ratingSlider != null)
            ratingSlider.value = entry.finalRating;

        UpdateStars(entry.finalRating);
    }

    private void SetupSlider()
    {
        if (ratingSlider == null)
            return;

        ratingSlider.minValue = 0f;
        ratingSlider.maxValue = 5f;
        ratingSlider.wholeNumbers = false;
    }

    private void SetupStars()
    {
        for (int i = 0; i < starFillImages.Length; i++)
        {
            if (starFillImages[i] == null)
                continue;

            starFillImages[i].type = Image.Type.Filled;
            starFillImages[i].fillMethod = Image.FillMethod.Horizontal;
            starFillImages[i].fillOrigin = (int)Image.OriginHorizontal.Left;
            starFillImages[i].fillAmount = 0f;
        }
    }

    private void UpdateStars(float rating)
    {
        for (int i = 0; i < starFillImages.Length; i++)
        {
            if (starFillImages[i] == null)
                continue;

            float fill = Mathf.Clamp(rating - i, 0f, 1f);
            starFillImages[i].fillAmount = fill;
        }
    }

    private string FormatHour(float totalMinutes)
    {
        int minutes = Mathf.FloorToInt(totalMinutes);
        int dayMinutes = minutes % 1440;
        int hours = dayMinutes / 60;
        int mins = dayMinutes % 60;

        return $"{hours:00}:{mins:00}";
    }
}