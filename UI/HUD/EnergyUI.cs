using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnergyUI : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private TMP_Text recoveryBonusText;
    [SerializeField] private TMP_Text debtPenaltyText;
    [SerializeField] private Slider energySlider;
    [SerializeField] private Image sliderFillImage;

    [Header("Cores da barra")]
    [SerializeField] private Color normalColor = new Color32(92, 140, 91, 255);
    [SerializeField] private Color warningColor = new Color32(201, 145, 58, 255);
    [SerializeField] private Color criticalColor = new Color32(156, 58, 58, 255);

    private readonly CultureInfo brazilCulture = new CultureInfo("pt-BR");
    private bool isBound;

    private void Start()
    {
        SetupSlider();
        StartCoroutine(BindRoutine());
    }

    private void OnDisable()
    {
        Unbind();
    }

    private IEnumerator BindRoutine()
    {
        while (PlayerEnergySystem.Instance == null)
            yield return null;

        Bind();
        RefreshAll();
    }

    private void SetupSlider()
    {
        if (energySlider == null)
            return;

        energySlider.minValue = 0f;
        energySlider.maxValue = 100f;
        energySlider.wholeNumbers = false;
    }

    private void Bind()
    {
        if (isBound)
            return;

        if (PlayerEnergySystem.Instance == null)
            return;

        PlayerEnergySystem.Instance.OnEnergyChanged += RefreshEnergyUI;
        PlayerEnergySystem.Instance.OnEnergyContextChanged += RefreshContextUI;
        isBound = true;
    }

    private void Unbind()
    {
        if (!isBound)
            return;

        if (PlayerEnergySystem.Instance != null)
        {
            PlayerEnergySystem.Instance.OnEnergyChanged -= RefreshEnergyUI;
            PlayerEnergySystem.Instance.OnEnergyContextChanged -= RefreshContextUI;
        }

        isBound = false;
    }

    private void RefreshAll()
    {
        if (PlayerEnergySystem.Instance == null)
            return;

        RefreshEnergyUI(PlayerEnergySystem.Instance.CurrentEnergy);
        RefreshContextUI();
    }

    private void RefreshEnergyUI(float currentEnergy)
    {
        if (PlayerEnergySystem.Instance == null)
            return;

        float maxEnergy = PlayerEnergySystem.Instance.MaxEnergy;

        if (energyText != null)
        {
            int current = Mathf.RoundToInt(currentEnergy);
            int max = Mathf.RoundToInt(maxEnergy);

            energyText.text = $"{current}/{max}";
        }

        if (energySlider != null)
        {
            energySlider.maxValue = maxEnergy;
            energySlider.value = currentEnergy;
        }

        UpdateStateText();
        UpdateSliderColor();
    }

    private void RefreshContextUI()
    {
        if (PlayerEnergySystem.Instance == null)
            return;

        if (recoveryBonusText != null)
        {
            if (!PlayerEnergySystem.Instance.IsInsideBarbershop &&
                !string.IsNullOrWhiteSpace(PlayerEnergySystem.Instance.CurrentRecoveryBonusText))
            {
                recoveryBonusText.text = PlayerEnergySystem.Instance.CurrentRecoveryBonusText;
            }
            else
            {
                recoveryBonusText.text = "";
            }
        }

        if (debtPenaltyText != null)
        {
            debtPenaltyText.text = PlayerEnergySystem.Instance.CurrentDebtPenaltyText;
        }

        UpdateStateText();
        UpdateSliderColor();
    }

    private void UpdateStateText()
    {
        if (stateText == null || PlayerEnergySystem.Instance == null)
            return;

        if (PlayerEnergySystem.Instance.IsCollapsed || PlayerEnergySystem.Instance.IsExhausted())
            stateText.text = "Exausto";
        else if (PlayerEnergySystem.Instance.IsCriticalEnergy())
            stateText.text = "Crítico";
        else if (PlayerEnergySystem.Instance.IsLowEnergy())
            stateText.text = "Cansado";
        else
            stateText.text = "Disposto";
    }

    private void UpdateSliderColor()
    {
        if (sliderFillImage == null || PlayerEnergySystem.Instance == null)
            return;

        if (PlayerEnergySystem.Instance.IsCollapsed || PlayerEnergySystem.Instance.IsExhausted() || PlayerEnergySystem.Instance.IsCriticalEnergy())
            sliderFillImage.color = criticalColor;
        else if (PlayerEnergySystem.Instance.IsLowEnergy())
            sliderFillImage.color = warningColor;
        else
            sliderFillImage.color = normalColor;
    }
}