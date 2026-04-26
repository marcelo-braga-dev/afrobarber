using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServicePriceManagementRowUI : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private ServiceType serviceType;

    [Header("UI")]
    [SerializeField] private TMP_Text serviceNameText;
    [SerializeField] private TMP_Text basePriceText;
    [SerializeField] private TMP_Text finalPriceText;
    [SerializeField] private TMP_InputField adjustmentPercentInput;
    [SerializeField] private Slider adjustmentSlider;

    private bool suppressCallbacks;

    public ServiceType ServiceType => serviceType;

    public void Setup(ServiceType type)
    {
        serviceType = type;
    }

    public void Bind(GlobalGameplayManagement management)
    {
        if (adjustmentPercentInput != null)
        {
            adjustmentPercentInput.onEndEdit.RemoveAllListeners();
            adjustmentPercentInput.onEndEdit.AddListener(OnAdjustmentInputSubmitted);
        }

        if (adjustmentSlider != null)
        {
            adjustmentSlider.minValue = -80f;
            adjustmentSlider.maxValue = 300f;
            adjustmentSlider.wholeNumbers = false;
            adjustmentSlider.onValueChanged.RemoveAllListeners();
            adjustmentSlider.onValueChanged.AddListener(OnAdjustmentSliderChanged);
        }

        if (management != null)
        {
            if (serviceNameText != null)
                serviceNameText.text = management.GetDisplayNameForService(serviceType);

            RefreshFromManagement(management);
            return;
        }

        if (serviceNameText != null)
            serviceNameText.text = serviceType.ToString();
    }

    public void RefreshFromManagement(GlobalGameplayManagement management)
    {
        if (management == null)
            return;

        int basePrice = 0;
        if (management.GlobalPriceTable != null)
            basePrice = management.GlobalPriceTable.GetPrice(serviceType);

        int finalPrice = management.GetCurrentPriceForService(serviceType);
        float adjustment = management.GetAdjustmentForService(serviceType);

        if (basePriceText != null)
            basePriceText.text = $"Base: R$ {basePrice}";

        if (finalPriceText != null)
            finalPriceText.text = $"Final: R$ {finalPrice}";

        suppressCallbacks = true;

        float percent = adjustment * 100f;

        if (adjustmentPercentInput != null)
            adjustmentPercentInput.text = Mathf.RoundToInt(percent).ToString();

        if (adjustmentSlider != null)
            adjustmentSlider.value = percent;

        suppressCallbacks = false;
    }

    private void OnAdjustmentInputSubmitted(string inputValue)
    {
        if (suppressCallbacks)
            return;

        if (GlobalGameplayManagement.Instance == null)
            return;

        if (!float.TryParse(inputValue, out float value))
            value = 0f;

        value = Mathf.Clamp(value, -80f, 300f);
        GlobalGameplayManagement.Instance.SetAdjustmentForService(serviceType, value / 100f);

        if (adjustmentSlider != null)
            adjustmentSlider.value = value;

        RefreshFromManagement(GlobalGameplayManagement.Instance);
    }

    private void OnAdjustmentSliderChanged(float value)
    {
        if (suppressCallbacks)
            return;

        if (GlobalGameplayManagement.Instance == null)
            return;

        value = Mathf.Clamp(value, -80f, 300f);
        GlobalGameplayManagement.Instance.SetAdjustmentForService(serviceType, value / 100f);

        if (adjustmentPercentInput != null)
            adjustmentPercentInput.text = Mathf.RoundToInt(value).ToString();

        RefreshFromManagement(GlobalGameplayManagement.Instance);
    }
}