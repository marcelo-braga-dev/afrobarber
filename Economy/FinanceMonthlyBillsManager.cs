using System;
using System.Collections.Generic;
using UnityEngine;

public class FinanceMonthlyBillsManager : MonoBehaviour
{
    public static FinanceMonthlyBillsManager Instance { get; private set; }

    [Header("Configuração")]
    [SerializeField] private bool autoGenerateOnStart = true;

    [Header("Dívidas mensais")]
    [SerializeField] private List<MonthlyDebtDefinition> monthlyDebts = new List<MonthlyDebtDefinition>();

    [Header("Persistência horas trabalhadas")]
    [SerializeField] private string workedMinutesKeyPrefix = "AFROBARBER_WORKED_MINUTES_";

    private int lastObservedYear;
    private int lastObservedMonth;

    public IReadOnlyList<MonthlyDebtDefinition> MonthlyDebts => monthlyDebts;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        PersistentGameObject.MakePersistent(gameObject);
    }

    private void Start()
    {
        DateTime now = GetNow();
        lastObservedYear = now.Year;
        lastObservedMonth = now.Month;

        SubscribeToTimeSystem();

        if (autoGenerateOnStart)
            EnsureCurrentMonthDebts();
    }

    private void OnDestroy()
    {
        UnsubscribeFromTimeSystem();
    }

    private void SubscribeToTimeSystem()
    {
        if (GameTimeSystem.Instance != null)
            GameTimeSystem.Instance.onDayChanged.AddListener(OnGameDayChanged);
    }

    private void UnsubscribeFromTimeSystem()
    {
        if (GameTimeSystem.Instance != null)
            GameTimeSystem.Instance.onDayChanged.RemoveListener(OnGameDayChanged);
    }

    private DateTime GetNow()
    {
        if (GameTimeSystem.Instance != null)
            return GameTimeSystem.Instance.CurrentDateTime;

        return DateTime.Now;
    }

    private void OnGameDayChanged()
    {
        DateTime now = GetNow();

        if (now.Year == lastObservedYear && now.Month == lastObservedMonth)
            return;

        lastObservedYear = now.Year;
        lastObservedMonth = now.Month;

        EnsureCurrentMonthDebts();
    }

    public void EnsureCurrentMonthDebts()
    {
        DateTime now = GetNow();
        EnsureDebtsForMonth(now.Year, now.Month);
    }

    public void EnsureDebtsForMonth(int targetYear, int targetMonth)
    {
        if (FinanceManager.Instance == null)
        {
            Debug.LogWarning("[FinanceMonthlyBillsManager] FinanceManager.Instance não encontrado.");
            return;
        }

        for (int i = 0; i < monthlyDebts.Count; i++)
        {
            MonthlyDebtDefinition debt = monthlyDebts[i];

            if (debt == null || !debt.enabled)
                continue;

            debt.EnsureId();

            int workedMinutesUsed = 0;

            if (debt.calculationMode == MonthlyDebtCalculationMode.FixedPlusWorkedHours)
            {
                DateTime usageMonthDate = new DateTime(targetYear, targetMonth, 1);

                if (debt.usePreviousMonthWorkedHours)
                    usageMonthDate = usageMonthDate.AddMonths(-1);

                workedMinutesUsed = GetWorkedMinutes(usageMonthDate.Year, usageMonthDate.Month);
            }

            FinanceManager.Instance.AddAutoMonthlyExpense(
                debt,
                targetYear,
                targetMonth,
                workedMinutesUsed
            );
        }
    }

    public void RegisterWorkedMinutes(int minutes)
    {
        if (minutes <= 0)
            return;

        DateTime now = GetNow();
        string key = BuildWorkedMinutesKey(now.Year, now.Month);

        int currentValue = PlayerPrefs.GetInt(key, 0);
        currentValue += minutes;

        PlayerPrefs.SetInt(key, currentValue);
        PlayerPrefs.Save();
    }

    public int GetWorkedMinutes(int year, int month)
    {
        string key = BuildWorkedMinutesKey(year, month);
        return PlayerPrefs.GetInt(key, 0);
    }

    public float GetWorkedHours(int year, int month)
    {
        return GetWorkedMinutes(year, month) / 60f;
    }

    private string BuildWorkedMinutesKey(int year, int month)
    {
        return $"{workedMinutesKeyPrefix}{year}_{month}";
    }

    [ContextMenu("Financeiro/Gerar dívidas do mês atual")]
    private void DebugGenerateCurrentMonth()
    {
        EnsureCurrentMonthDebts();
    }

    [ContextMenu("Financeiro/Criar contas padrão")]
    private void CreateDefaultBills()
    {
        monthlyDebts.Clear();

        monthlyDebts.Add(new MonthlyDebtDefinition
        {
            title = "Aluguel",
            description = "Aluguel mensal da barbearia",
            origin = FinanceMovementOrigin.Rent,
            dueDay = 5,
            calculationMode = MonthlyDebtCalculationMode.Fixed,
            baseAmount = 1800,
            annualIncreasePercent = 8f,
            baseYear = 2026,
            baseMonth = 1,
            pricePerWorkedHour = 0f,
            usePreviousMonthWorkedHours = false
        });

        monthlyDebts.Add(new MonthlyDebtDefinition
        {
            title = "Conta de energia",
            description = "Energia mensal da barbearia",
            origin = FinanceMovementOrigin.Electricity,
            dueDay = 10,
            calculationMode = MonthlyDebtCalculationMode.FixedPlusWorkedHours,
            baseAmount = 220,
            annualIncreasePercent = 6f,
            baseYear = 2026,
            baseMonth = 1,
            pricePerWorkedHour = 7f,
            usePreviousMonthWorkedHours = true
        });

        monthlyDebts.Add(new MonthlyDebtDefinition
        {
            title = "Conta de água",
            description = "Água mensal da barbearia",
            origin = FinanceMovementOrigin.Water,
            dueDay = 12,
            calculationMode = MonthlyDebtCalculationMode.FixedPlusWorkedHours,
            baseAmount = 90,
            annualIncreasePercent = 5f,
            baseYear = 2026,
            baseMonth = 1,
            pricePerWorkedHour = 3f,
            usePreviousMonthWorkedHours = true
        });
    }
}