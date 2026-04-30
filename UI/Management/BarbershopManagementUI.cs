using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BarbershopManagementUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject rootPanel;

    [Header("Dropdowns de horário")]
    [SerializeField] private TMP_Dropdown openingHourDropdown;
    [SerializeField] private TMP_Dropdown openingMinuteDropdown;
    [SerializeField] private TMP_Dropdown closingHourDropdown;
    [SerializeField] private TMP_Dropdown closingMinuteDropdown;

    [Header("Configuração dos minutos")]
    [Tooltip("Use 1 para listar todos os minutos de 00 a 59. Use 5 para listar 00, 05, 10, 15...")]
    [SerializeField] private int minuteStep = 5;

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

    private bool dropdownsInitialized;

    private void Awake()
    {
        InitializeTimeDropdowns();

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

        InitializeTimeDropdowns();

        GlobalGameplayManagement management = GlobalGameplayManagement.Instance;

        for (int i = 0; i < createdRows.Count; i++)
            createdRows[i].Bind(management);

        SetHourDropdownValue(openingHourDropdown, management.GetOpeningHour());
        SetMinuteDropdownValue(openingMinuteDropdown, management.GetOpeningMinute());

        SetHourDropdownValue(closingHourDropdown, management.GetClosingHour());
        SetMinuteDropdownValue(closingMinuteDropdown, management.GetClosingMinute());

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
            ServiceType type = types[i];

            ServicePriceManagementRowUI row = Instantiate(rowPrefab, priceRowsParent);
            row.Setup(type);
            row.Bind(GlobalGameplayManagement.Instance);
            createdRows.Add(row);
        }
    }

    private void OnClickSave()
    {
        if (GlobalGameplayManagement.Instance == null)
            return;

        GlobalGameplayManagement management = GlobalGameplayManagement.Instance;

        int openHour = GetHourDropdownValue(openingHourDropdown, management.GetOpeningHour());
        int openMinute = GetMinuteDropdownValue(openingMinuteDropdown, management.GetOpeningMinute());

        int closeHour = GetHourDropdownValue(closingHourDropdown, management.GetClosingHour());
        int closeMinute = GetMinuteDropdownValue(closingMinuteDropdown, management.GetClosingMinute());

        if (!IsValidBusinessWindow(openHour, openMinute, closeHour, closeMinute))
        {
            Debug.LogWarning("[Gestão] Horário inválido. O fechamento precisa ser depois da abertura.");
            RefreshAll();
            return;
        }

        if (!HasAnyWorkDaySelected())
        {
            Debug.LogWarning("[Gestão] Selecione pelo menos um dia de funcionamento.");
            RefreshAll();
            return;
        }

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

            businessStatusText.text =
                $"Barbearia: {status} ({GameTimeSystem.Instance.OpeningHour:00}:{GameTimeSystem.Instance.OpeningMinute:00} - {GameTimeSystem.Instance.ClosingHour:00}:{GameTimeSystem.Instance.ClosingMinute:00})";
        }

        if (GlobalGameplayManagement.Instance != null && overworkMultiplierText != null)
        {
            float multiplier = GlobalGameplayManagement.Instance.GetOverworkEnergyMultiplier();
            overworkMultiplierText.text = $"Cansaço: x{multiplier:0.00}";
        }

        if (demandHintText != null)
            demandHintText.text = "Demanda: preços acima do sugerido reduzem fluxo; abaixo aumentam.";
    }

    private void InitializeTimeDropdowns()
    {
        if (dropdownsInitialized)
            return;

        minuteStep = Mathf.Clamp(minuteStep, 1, 30);

        SetupNumberDropdown(openingHourDropdown, 0, 23, 1);
        SetupNumberDropdown(closingHourDropdown, 0, 23, 1);

        SetupNumberDropdown(openingMinuteDropdown, 0, 59, minuteStep);
        SetupNumberDropdown(closingMinuteDropdown, 0, 59, minuteStep);

        dropdownsInitialized = true;
    }

    private static void SetupNumberDropdown(TMP_Dropdown dropdown, int min, int max, int step)
    {
        if (dropdown == null)
            return;

        dropdown.ClearOptions();

        List<string> options = new List<string>();

        for (int value = min; value <= max; value += Mathf.Max(1, step))
            options.Add(value.ToString("00"));

        dropdown.AddOptions(options);
        dropdown.RefreshShownValue();
    }

    private static void SetHourDropdownValue(TMP_Dropdown dropdown, int hour)
    {
        if (dropdown == null || dropdown.options == null || dropdown.options.Count == 0)
            return;

        int value = Mathf.Clamp(hour, 0, 23);
        int index = Mathf.Clamp(value, 0, dropdown.options.Count - 1);

        dropdown.SetValueWithoutNotify(index);
        dropdown.RefreshShownValue();
    }

    private void SetMinuteDropdownValue(TMP_Dropdown dropdown, int minute)
    {
        if (dropdown == null || dropdown.options == null || dropdown.options.Count == 0)
            return;

        minute = Mathf.Clamp(minute, 0, 59);

        int index = Mathf.RoundToInt((float)minute / Mathf.Max(1, minuteStep));
        index = Mathf.Clamp(index, 0, dropdown.options.Count - 1);

        dropdown.SetValueWithoutNotify(index);
        dropdown.RefreshShownValue();
    }

    private static int GetHourDropdownValue(TMP_Dropdown dropdown, int fallback)
    {
        if (dropdown == null || dropdown.options == null || dropdown.options.Count == 0)
            return Mathf.Clamp(fallback, 0, 23);

        return Mathf.Clamp(dropdown.value, 0, 23);
    }

    private int GetMinuteDropdownValue(TMP_Dropdown dropdown, int fallback)
    {
        if (dropdown == null || dropdown.options == null || dropdown.options.Count == 0)
            return Mathf.Clamp(fallback, 0, 59);

        int minute = dropdown.value * Mathf.Max(1, minuteStep);
        return Mathf.Clamp(minute, 0, 59);
    }

    private bool IsValidBusinessWindow(int openHour, int openMinute, int closeHour, int closeMinute)
    {
        int openingTotal = (openHour * 60) + openMinute;
        int closingTotal = (closeHour * 60) + closeMinute;

        return closingTotal > openingTotal;
    }

    private bool HasAnyWorkDaySelected()
    {
        return GetToggleValue(mondayToggle) ||
               GetToggleValue(tuesdayToggle) ||
               GetToggleValue(wednesdayToggle) ||
               GetToggleValue(thursdayToggle) ||
               GetToggleValue(fridayToggle) ||
               GetToggleValue(saturdayToggle) ||
               GetToggleValue(sundayToggle);
    }

    private static void SetToggle(Toggle toggle, bool value)
    {
        if (toggle != null)
            toggle.SetIsOnWithoutNotify(value);
    }

    private static bool GetToggleValue(Toggle toggle)
    {
        return toggle != null && toggle.isOn;
    }
}