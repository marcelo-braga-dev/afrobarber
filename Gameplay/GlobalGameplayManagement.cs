using System;
using UnityEngine;

public class GlobalGameplayManagement : MonoBehaviour
{
    public static GlobalGameplayManagement Instance { get; private set; }

    [Serializable]
    private class RuntimePriceAdjustment
    {
        public ServiceType serviceType;
        [Range(-0.8f, 3f)] public float adjustmentPercent;
    }

    [Header("Tabela de preços global")]
    [SerializeField] private ServicePriceTable globalPriceTable;
    [SerializeField] private bool overrideRequestPriceTable = true;
    [SerializeField] private RuntimePriceAdjustment[] categoryAdjustments;

    [Header("Política de precificação")]
    [SerializeField] private float minSuggestedMultiplierByRating = 0.75f;
    [SerializeField] private float maxSuggestedMultiplierByRating = 1.45f;

    [Header("Horário sugerido")]
    [SerializeField] private bool suggestedMonday = false;
    [SerializeField] private bool suggestedTuesday = true;
    [SerializeField] private bool suggestedWednesday = true;
    [SerializeField] private bool suggestedThursday = true;
    [SerializeField] private bool suggestedFriday = true;
    [SerializeField] private bool suggestedSaturday = true;
    [SerializeField] private bool suggestedSunday = false;
    [SerializeField] private int suggestedOpeningHour = 10;
    [SerializeField] private int suggestedOpeningMinute = 0;
    [SerializeField] private int suggestedClosingHour = 19;
    [SerializeField] private int suggestedClosingMinute = 0;

    [Header("Aplicação automática")]
    [SerializeField] private bool applySuggestedScheduleOnStart = true;
    [SerializeField] private bool applySuggestedWorkDaysOnStart = true;

    [Header("Impacto na gameplay")]
    [SerializeField] private float energyPenaltyPerExtraHour = 0.08f;
    [SerializeField] private float spawnPenaltyPerPriceDelta = 0.15f;
    [SerializeField] private float spawnBonusPerPriceDelta = 0.10f;

    public event Action OnBusinessSettingsChanged;
    public event Action OnPricesChanged;
    public event Action OnScheduleChanged;

    public ServicePriceTable GlobalPriceTable => globalPriceTable;
    public bool OverrideRequestPriceTable => overrideRequestPriceTable;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ApplySuggestedScheduleIfNeeded();
    }

    public void ApplySuggestedScheduleIfNeeded()
    {
        if (GameTimeSystem.Instance == null)
            return;

        if (applySuggestedScheduleOnStart)
        {
            ApplyScheduleToGameTime(
                suggestedOpeningHour,
                suggestedOpeningMinute,
                suggestedClosingHour,
                suggestedClosingMinute
            );
        }

        if (applySuggestedWorkDaysOnStart)
        {
            ApplyWorkDaysToGameTime();
        }
    }

    public void SetSuggestedBusinessHours(int openingHour, int openingMinute, int closingHour, int closingMinute)
    {
        SanitizeBusinessHours(ref openingHour, ref openingMinute, ref closingHour, ref closingMinute);

        bool changed =
            suggestedOpeningHour != openingHour ||
            suggestedOpeningMinute != openingMinute ||
            suggestedClosingHour != closingHour ||
            suggestedClosingMinute != closingMinute;

        suggestedOpeningHour = openingHour;
        suggestedOpeningMinute = openingMinute;
        suggestedClosingHour = closingHour;
        suggestedClosingMinute = closingMinute;

        ApplyScheduleToGameTime(openingHour, openingMinute, closingHour, closingMinute);

        if (changed)
        {
            OnScheduleChanged?.Invoke();
            OnBusinessSettingsChanged?.Invoke();
        }
    }

    public void SetSuggestedWorkDays(
        bool monday,
        bool tuesday,
        bool wednesday,
        bool thursday,
        bool friday,
        bool saturday,
        bool sunday)
    {
        EnsureAtLeastOneWorkDay(ref monday, ref tuesday, ref wednesday, ref thursday, ref friday, ref saturday, ref sunday);

        bool changed =
            suggestedMonday != monday ||
            suggestedTuesday != tuesday ||
            suggestedWednesday != wednesday ||
            suggestedThursday != thursday ||
            suggestedFriday != friday ||
            suggestedSaturday != saturday ||
            suggestedSunday != sunday;

        suggestedMonday = monday;
        suggestedTuesday = tuesday;
        suggestedWednesday = wednesday;
        suggestedThursday = thursday;
        suggestedFriday = friday;
        suggestedSaturday = saturday;
        suggestedSunday = sunday;

        ApplyWorkDaysToGameTime();

        if (changed)
        {
            OnScheduleChanged?.Invoke();
            OnBusinessSettingsChanged?.Invoke();
        }
    }

    public int GetOpeningHour() => suggestedOpeningHour;
    public int GetOpeningMinute() => suggestedOpeningMinute;
    public int GetClosingHour() => suggestedClosingHour;
    public int GetClosingMinute() => suggestedClosingMinute;

    public bool GetSuggestedMonday() => suggestedMonday;
    public bool GetSuggestedTuesday() => suggestedTuesday;
    public bool GetSuggestedWednesday() => suggestedWednesday;
    public bool GetSuggestedThursday() => suggestedThursday;
    public bool GetSuggestedFriday() => suggestedFriday;
    public bool GetSuggestedSaturday() => suggestedSaturday;
    public bool GetSuggestedSunday() => suggestedSunday;

    public float GetAdjustmentForService(ServiceType serviceType)
    {
        return GetAdjustmentPercent(serviceType);
    }

    public void SetAdjustmentForService(ServiceType serviceType, float adjustmentPercent)
    {
        adjustmentPercent = Mathf.Clamp(adjustmentPercent, -0.8f, 3f);

        if (categoryAdjustments == null)
            categoryAdjustments = Array.Empty<RuntimePriceAdjustment>();

        for (int i = 0; i < categoryAdjustments.Length; i++)
        {
            RuntimePriceAdjustment entry = categoryAdjustments[i];
            if (entry != null && entry.serviceType == serviceType)
            {
                if (Mathf.Approximately(entry.adjustmentPercent, adjustmentPercent))
                    return;

                entry.adjustmentPercent = adjustmentPercent;
                NotifyPriceSettingsChanged();
                return;
            }
        }

        RuntimePriceAdjustment[] old = categoryAdjustments;
        RuntimePriceAdjustment[] updated = new RuntimePriceAdjustment[old.Length + 1];

        for (int i = 0; i < old.Length; i++)
            updated[i] = old[i];

        updated[old.Length] = new RuntimePriceAdjustment
        {
            serviceType = serviceType,
            adjustmentPercent = adjustmentPercent
        };

        categoryAdjustments = updated;
        NotifyPriceSettingsChanged();
    }

    public int GetCurrentPriceForService(ServiceType serviceType)
    {
        int basePrice = 0;

        if (globalPriceTable != null)
            basePrice = globalPriceTable.GetPrice(serviceType);

        float adjustment = GetAdjustmentPercent(serviceType);
        return Mathf.Max(0, Mathf.RoundToInt(basePrice * (1f + adjustment)));
    }

    public string GetDisplayNameForService(ServiceType serviceType)
    {
        if (globalPriceTable != null)
            return globalPriceTable.GetDisplayName(serviceType);

        return serviceType.ToString();
    }

    public int CalculateFinalPriceForRequest(ClientRequestData request)
    {
        if (request == null)
            return 0;

        int total = CalculateBaseCategoryPrice(request);
        return Mathf.Max(0, total);
    }

    public int CalculateSuggestedPriceForRequest(ClientRequestData request)
    {
        int basePrice = CalculateBaseCategoryPrice(request);
        float multiplier = GetSuggestedMultiplierFromRating();
        return Mathf.Max(0, Mathf.RoundToInt(basePrice * multiplier));
    }

    public float GetPriceSatisfactionScore(int finalPrice, int suggestedPrice)
    {
        if (suggestedPrice <= 0)
            return 1f;

        float ratio = (float)finalPrice / suggestedPrice;

        if (ratio > 1f)
        {
            float above = ratio - 1f;
            return Mathf.Clamp01(1f - (above * 1.2f));
        }

        float below = 1f - ratio;
        return Mathf.Clamp01(1f + (below * 0.6f));
    }

    public float GetSpawnDemandMultiplierFromLastService(int finalPrice, int suggestedPrice)
    {
        if (suggestedPrice <= 0)
            return 1f;

        float ratio = (float)finalPrice / suggestedPrice;

        if (ratio > 1f)
        {
            float above = ratio - 1f;
            return Mathf.Clamp(1f - (above * spawnPenaltyPerPriceDelta), 0.35f, 1f);
        }

        float below = 1f - ratio;
        return Mathf.Clamp(1f + (below * spawnBonusPerPriceDelta), 1f, 1.6f);
    }

    public float GetOverworkEnergyMultiplier()
    {
        if (GameTimeSystem.Instance == null)
            return 1f;

        float currentWindow = GetCurrentBusinessWindowHours();
        float suggestedWindow = GetSuggestedBusinessWindowHours();

        if (currentWindow <= suggestedWindow)
            return 1f;

        float extraHours = currentWindow - suggestedWindow;
        return Mathf.Max(1f, 1f + (extraHours * energyPenaltyPerExtraHour));
    }

    public void ForceNotifyBusinessSettingsChanged()
    {
        OnBusinessSettingsChanged?.Invoke();
    }

    private void ApplyScheduleToGameTime(int openHour, int openMinute, int closeHour, int closeMinute)
    {
        if (GameTimeSystem.Instance == null)
            return;

        GameTimeSystem.Instance.SetBusinessHours(openHour, openMinute, closeHour, closeMinute);
    }

    private void ApplyWorkDaysToGameTime()
    {
        if (GameTimeSystem.Instance == null)
            return;

        GameTimeSystem.Instance.SetWorkDays(
            suggestedMonday,
            suggestedTuesday,
            suggestedWednesday,
            suggestedThursday,
            suggestedFriday,
            suggestedSaturday,
            suggestedSunday
        );
    }

    private void NotifyPriceSettingsChanged()
    {
        OnPricesChanged?.Invoke();
        OnBusinessSettingsChanged?.Invoke();
    }

    private int CalculateBaseCategoryPrice(ClientRequestData request)
    {
        int total = GetCategoryPrice(request, request.serviceType, request.customServiceId);

        if (request.additionalServiceTypes != null)
        {
            for (int i = 0; i < request.additionalServiceTypes.Count; i++)
            {
                total += GetCategoryPrice(request, request.additionalServiceTypes[i], string.Empty);
            }
        }

        return Mathf.Max(0, total);
    }

    private int GetCategoryPrice(ClientRequestData request, ServiceType serviceType, string customServiceId)
    {
        ServicePriceTable sourceTable = ResolvePriceTable(request);
        int basePrice = sourceTable != null
            ? sourceTable.GetPrice(serviceType, customServiceId)
            : request.GetBaseTablePriceWithoutGlobalManagement();

        float adjustmentPercent = GetAdjustmentPercent(serviceType);
        float adjusted = basePrice * (1f + adjustmentPercent);

        return Mathf.Max(0, Mathf.RoundToInt(adjusted));
    }

    private ServicePriceTable ResolvePriceTable(ClientRequestData request)
    {
        if (overrideRequestPriceTable && globalPriceTable != null)
            return globalPriceTable;

        if (request != null && request.priceTable != null)
            return request.priceTable;

        return globalPriceTable;
    }

    private float GetAdjustmentPercent(ServiceType serviceType)
    {
        if (categoryAdjustments == null)
            return 0f;

        for (int i = 0; i < categoryAdjustments.Length; i++)
        {
            RuntimePriceAdjustment entry = categoryAdjustments[i];
            if (entry != null && entry.serviceType == serviceType)
                return entry.adjustmentPercent;
        }

        return 0f;
    }

    private float GetSuggestedMultiplierFromRating()
    {
        float rating = 3f;

        if (BarbershopRatingManager.Instance != null)
            rating = Mathf.Clamp(BarbershopRatingManager.Instance.GlobalRating, 0f, 5f);

        float t = rating / 5f;
        return Mathf.Lerp(minSuggestedMultiplierByRating, maxSuggestedMultiplierByRating, t);
    }

    private float GetCurrentBusinessWindowHours()
    {
        if (GameTimeSystem.Instance == null)
            return 0f;

        return Mathf.Max(0f, (GameTimeSystem.Instance.ClosingTotalMinutes - GameTimeSystem.Instance.OpeningTotalMinutes) / 60f);
    }

    private float GetSuggestedBusinessWindowHours()
    {
        int suggestedOpen = (suggestedOpeningHour * 60) + suggestedOpeningMinute;
        int suggestedClose = (suggestedClosingHour * 60) + suggestedClosingMinute;
        return Mathf.Max(0f, (suggestedClose - suggestedOpen) / 60f);
    }

    private void SanitizeBusinessHours(ref int openingHour, ref int openingMinute, ref int closingHour, ref int closingMinute)
    {
        openingHour = Mathf.Clamp(openingHour, 0, 23);
        openingMinute = Mathf.Clamp(openingMinute, 0, 59);
        closingHour = Mathf.Clamp(closingHour, 0, 23);
        closingMinute = Mathf.Clamp(closingMinute, 0, 59);

        int openTotal = openingHour * 60 + openingMinute;
        int closeTotal = closingHour * 60 + closingMinute;

        if (closeTotal <= openTotal)
        {
            closeTotal = Mathf.Min((openTotal + 60), (23 * 60) + 59);
            closingHour = closeTotal / 60;
            closingMinute = closeTotal % 60;
        }
    }

    private void EnsureAtLeastOneWorkDay(
        ref bool monday,
        ref bool tuesday,
        ref bool wednesday,
        ref bool thursday,
        ref bool friday,
        ref bool saturday,
        ref bool sunday)
    {
        bool any = monday || tuesday || wednesday || thursday || friday || saturday || sunday;
        if (!any)
            monday = true;
    }
}