using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BarbershopManagementUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject rootPanel;

    [Header("Horário")]
    [SerializeField] private TMP_InputField openingHourInput;
    [SerializeField] private TMP_InputField openingMinuteInput;
    [SerializeField] private TMP_InputField closingHourInput;
    [SerializeField] private TMP_InputField closingMinuteInput;

    [Header("Dias")]
    [SerializeField] private Toggle mondayToggle;
    [SerializeField] private Toggle tuesdayToggle;
    [SerializeField] private Toggle wednesdayToggle;
    [SerializeField] private Toggle thursdayToggle;
    [SerializeField] private Toggle fridayToggle;
    [SerializeField] private Toggle saturdayToggle;
    [SerializeField] private Toggle sundayToggle;

    [Header("Preços")]
    [SerializeField] private Transform priceRowsParent;
    [SerializeField] private ServicePriceManagementRowUI rowPrefab;

    [Header("Resumo")]
    [SerializeField] private TMP_Text businessStatusText;
    [SerializeField] private TMP_Text overworkMultiplierText;
    [SerializeField] private TMP_Text demandHintText;

    [Header("Ações")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private BarbershopManagementSaveSystem saveSystem;

    private readonly List<ServicePriceManagementRowUI> createdRows = new List<ServicePriceManagementRowUI>();

    private void Awake()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(OnClickSave);

        if (resetButton != null)
            resetButton.onClick.AddListener(OnClickReset);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        BuildPriceRowsIfNeeded();
    }

    private void OnEnable()
    {
        if (GlobalGameplayManagement.Instance != null)
            GlobalGameplayManagement.Instance.OnBusinessSettingsChanged += RefreshAll;

        RefreshAll();
    }

    private void OnDisable()
    {
        if (GlobalGameplayManagement.Instance != null)
            GlobalGameplayManagement.Instance.OnBusinessSettingsChanged -= RefreshAll;
    }

    public void Open()
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);

        RefreshAll();
    }

    public void Close()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    public void RefreshAll()
    {
        if (GlobalGameplayManagement.Instance == null)
            return;

        GlobalGameplayManagement management = GlobalGameplayManagement.Instance;

        for (int i = 0; i < createdRows.Count; i++)
            createdRows[i].Bind(management);

        SetInputValue(openingHourInput, management.GetOpeningHour());
        SetInputValue(openingMinuteInput, management.GetOpeningMinute());
        SetInputValue(closingHourInput, management.GetClosingHour());
        SetInputValue(closingMinuteInput, management.GetClosingMinute());

        SetToggle(mondayToggle, management.GetSuggestedMonday());
        SetToggle(tuesdayToggle, management.GetSuggestedTuesday());
        SetToggle(wednesdayToggle, management.GetSuggestedWednesday());
        SetToggle(thursdayToggle, management.GetSuggestedThursday());
        SetToggle(fridayToggle, management.GetSuggestedFriday());
        SetToggle(saturdayToggle, management.GetSuggestedSaturday());
        SetToggle(sundayToggle, management.GetSuggestedSunday());

        for (int i = 0; i < createdRows.Count; i++)
            createdRows[i].RefreshFromManagement(management);

        RefreshSummary();
    }

    private void BuildPriceRowsIfNeeded()
    {
        if (rowPrefab == null || priceRowsParent == null || createdRows.Count > 0)
            return;

        ServiceType[] types = (ServiceType[])System.Enum.GetValues(typeof(ServiceType));
        for (int i = 0; i < types.Length; i++)
        {
            ServicePriceManagementRowUI row = Instantiate(rowPrefab, priceRowsParent);
            row.Setup(types[i]);
            row.Bind(GlobalGameplayManagement.Instance);
            createdRows.Add(row);
        }
    }

    private void OnClickSave()
    {
        if (GlobalGameplayManagement.Instance == null)
            return;

        GlobalGameplayManagement management = GlobalGameplayManagement.Instance;

        int openHour = ParseInt(openingHourInput, management.GetOpeningHour(), 0, 23);
        int openMinute = ParseInt(openingMinuteInput, management.GetOpeningMinute(), 0, 59);
        int closeHour = ParseInt(closingHourInput, management.GetClosingHour(), 0, 23);
        int closeMinute = ParseInt(closingMinuteInput, management.GetClosingMinute(), 0, 59);

        management.SetSuggestedBusinessHours(openHour, openMinute, closeHour, closeMinute);

        management.SetSuggestedWorkDays(
            GetToggleValue(mondayToggle),
            GetToggleValue(tuesdayToggle),
            GetToggleValue(wednesdayToggle),
            GetToggleValue(thursdayToggle),
            GetToggleValue(fridayToggle),
            GetToggleValue(saturdayToggle),
            GetToggleValue(sundayToggle)
        );

        if (saveSystem != null)
            saveSystem.Save();

        RefreshAll();
    }

    private void OnClickReset()
    {
        if (saveSystem != null)
            saveSystem.Load();

        RefreshAll();
    }

    private void RefreshSummary()
    {
        if (GameTimeSystem.Instance != null && businessStatusText != null)
        {
            string status = GameTimeSystem.Instance.IsWorkDay && GameTimeSystem.Instance.IsWithinBusinessHours
                ? "Aberta"
                : "Fechada";

            businessStatusText.text = $"Barbearia: {status} ({GameTimeSystem.Instance.OpeningHour:00}:{GameTimeSystem.Instance.OpeningMinute:00} - {GameTimeSystem.Instance.ClosingHour:00}:{GameTimeSystem.Instance.ClosingMinute:00})";
        }

        if (GlobalGameplayManagement.Instance != null && overworkMultiplierText != null)
        {
            float multiplier = GlobalGameplayManagement.Instance.GetOverworkEnergyMultiplier();
            overworkMultiplierText.text = $"Cansaço: x{multiplier:0.00}";
        }

        if (demandHintText != null)
            demandHintText.text = "Demanda: preços acima do sugerido reduzem fluxo; abaixo aumentam.";
    }

    private static void SetInputValue(TMP_InputField field, int value)
    {
        if (field != null)
            field.text = value.ToString("00");
    }

    private static void SetToggle(Toggle toggle, bool value)
    {
        if (toggle != null)
            toggle.isOn = value;
    }

    private static bool GetToggleValue(Toggle toggle)
    {
        return toggle != null && toggle.isOn;
    }

    private static int ParseInt(TMP_InputField field, int fallback, int min, int max)
    {
        if (field == null)
            return Mathf.Clamp(fallback, min, max);

        if (!int.TryParse(field.text, out int value))
            value = fallback;

        return Mathf.Clamp(value, min, max);
    }
}