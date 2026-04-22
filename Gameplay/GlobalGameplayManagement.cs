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
            GameTimeSystem.Instance.SetBusinessHours(
                suggestedOpeningHour,
                suggestedOpeningMinute,
                suggestedClosingHour,
                suggestedClosingMinute
            );
        }

        if (applySuggestedWorkDaysOnStart)
        {
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
}
