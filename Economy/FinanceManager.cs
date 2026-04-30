using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FinanceManager : MonoBehaviour
{
    public static FinanceManager Instance { get; private set; }

    [Header("Persistência")]
    [SerializeField] private bool usePlayerPrefs = true;
    [SerializeField] private string historySaveKey = "AFROBARBER_FINANCE_HISTORY";
    [SerializeField] private string currentCashSaveKey = "AFROBARBER_FINANCE_CURRENT_CASH";
    [SerializeField] private int defaultStartingCash = 1000;

    [Header("Migração")]
    [SerializeField] private bool migrateLegacyMoney = true;
    [SerializeField] private string legacyPlayerMoneyKey = "AFROBARBER_PLAYER_MONEY";
    [SerializeField] private string legacyCashRegisterKey = "AFROBARBER_CASH_REGISTER_MONEY";
    [SerializeField] private string legacyMigrationDoneKey = "AFROBARBER_FINANCE_MIGRATION_V2_DONE";

    [Header("UI - Caixa e Dívida")]
    [SerializeField] private TMP_Text caixaAtualText;
    [SerializeField] private TMP_Text dividaAtualText;
    [SerializeField] private string moneyPrefix = "R$ ";

    [Header("Visual da UI")]
    [SerializeField] private bool useDynamicColor = true;
    [SerializeField] private Color positiveColor = new Color(0.2f, 0.9f, 0.2f);
    [SerializeField] private Color negativeColor = new Color(0.9f, 0.2f, 0.2f);

    [Header("Debug")]
    [SerializeField] private bool logMessagesInConsole = true;

    private readonly List<FinanceMovementData> movements = new List<FinanceMovementData>();
    private int currentCash;

    public IReadOnlyList<FinanceMovementData> Movements => movements;
    public int CurrentCash => currentCash;

    public event Action OnFinanceDataChanged;
    public event Action<int> OnCashChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[FinanceManager] Instância duplicada encontrada. O objeto duplicado será destruído.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        PersistentGameObject.MakePersistent(gameObject);

        TryMigrateLegacyCash();
        LoadCash();
        LoadData();
    }

    private void Start()
    {
        NotifyChanged();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public DateTime GetNow()
    {
        if (GameTimeSystem.Instance != null)
            return GameTimeSystem.Instance.CurrentDateTime;

        return DateTime.Now;
    }

    public string FormatDateTime(DateTime dateTime)
    {
        if (GameTimeSystem.Instance != null)
            return GameTimeSystem.Instance.FormatDateTime(dateTime);

        return dateTime.ToString("dd/MM/yyyy HH:mm");
    }

    public bool HasEnoughMoney(int amount)
    {
        if (amount <= 0)
            return true;

        return currentCash >= amount;
    }

    public void AddMoney(int amount, string reason = "")
    {
        if (amount <= 0)
            return;

        AddMoneyInternal(amount);
        NotifyChanged();

        if (logMessagesInConsole)
            Debug.Log($"[FinanceManager] Dinheiro adicionado: +R$ {amount}. Saldo atual: R$ {currentCash}. {reason}");
    }

    public bool SpendMoney(int amount, string reason = "")
    {
        if (amount <= 0)
            return true;

        bool spent = SpendMoneyInternal(amount);

        if (!spent)
            return false;

        NotifyChanged();

        if (logMessagesInConsole)
            Debug.Log($"[FinanceManager] Dinheiro gasto: -R$ {amount}. Saldo atual: R$ {currentCash}. {reason}");

        return true;
    }

    public void SetCurrentCash(int amount)
    {
        currentCash = Mathf.Max(0, amount);
        SaveCash();
        NotifyChanged();

        if (logMessagesInConsole)
            Debug.Log($"[FinanceManager] Saldo ajustado manualmente para R$ {currentCash}.");
    }

    public void ResetCash(int amount = 0)
    {
        currentCash = Mathf.Max(0, amount);
        SaveCash();
        NotifyChanged();

        if (logMessagesInConsole)
            Debug.Log($"[FinanceManager] Saldo resetado para R$ {currentCash}.");
    }

    public FinanceMovementData RegisterServiceIncome(ClientRequestData requestData, string clientName = "")
    {
        if (requestData == null)
        {
            Debug.LogWarning("[FinanceManager] RequestData nulo.");
            return null;
        }

        int amount = Mathf.Max(0, requestData.ServicePrice);

        if (amount <= 0)
            return null;

        DateTime now = GetNow();

        FinanceMovementData movement = new FinanceMovementData
        {
            id = Guid.NewGuid().ToString("N"),
            movementType = FinanceMovementType.Income,
            origin = FinanceMovementOrigin.Service,
            title = string.IsNullOrWhiteSpace(requestData.RequestName) ? "Atendimento" : requestData.RequestName,
            description = requestData.GetDescription(),
            amount = amount,
            paymentStatus = FinancePaymentStatus.None,
            relatedRequestId = requestData.RequestId,
            relatedClientName = clientName,
            CreatedAt = now
        };

        movements.Add(movement);
        SortMovements();
        SaveData();

        AddMoneyInternal(amount);
        NotifyChanged();

        if (logMessagesInConsole)
            Debug.Log($"[FinanceManager] Receita de atendimento registrada: +R$ {amount} | Caixa atual: R$ {currentCash}");

        return movement;
    }

    public FinanceMovementData AddCashIncome(
        string title,
        string description,
        int amount,
        FinanceMovementOrigin origin = FinanceMovementOrigin.Service)
    {
        amount = Mathf.Max(0, amount);

        if (amount <= 0)
            return null;

        if (string.IsNullOrWhiteSpace(title))
            title = "Entrada";

        DateTime now = GetNow();

        FinanceMovementData movement = new FinanceMovementData
        {
            id = Guid.NewGuid().ToString("N"),
            movementType = FinanceMovementType.Income,
            origin = origin,
            title = title,
            description = description,
            amount = amount,
            paymentStatus = FinancePaymentStatus.None,
            CreatedAt = now
        };

        movements.Add(movement);
        SortMovements();
        SaveData();

        AddMoneyInternal(amount);
        NotifyChanged();

        if (logMessagesInConsole)
            Debug.Log($"[FinanceManager] Entrada registrada: +R$ {amount} | Caixa atual: R$ {currentCash}");

        return movement;
    }

    public FinanceMovementData AddExpense(
        string title,
        string description,
        FinanceMovementOrigin origin,
        int amount,
        DateTime dueDate)
    {
        return AddExpenseInternal(
            title,
            description,
            origin,
            amount,
            dueDate,
            FinancePaymentStatus.Open,
            false,
            string.Empty,
            0,
            0,
            0,
            0,
            0,
            null,
            true
        );
    }

    public FinanceMovementData AddAutoMonthlyExpense(
        MonthlyDebtDefinition definition,
        int referenceYear,
        int referenceMonth,
        int workedMinutesUsedInCalculation)
    {
        if (definition == null || !definition.enabled)
            return null;

        definition.EnsureId();

        if (HasAutoExpenseForMonth(definition.recurringId, referenceYear, referenceMonth))
            return null;

        int amount = definition.CalculateAmount(referenceYear, referenceMonth, workedMinutesUsedInCalculation);
        int dueDay = Mathf.Clamp(definition.dueDay, 1, DateTime.DaysInMonth(referenceYear, referenceMonth));

        DateTime dueDate = new DateTime(referenceYear, referenceMonth, dueDay, 8, 0, 0);
        DateTime createdAt = new DateTime(referenceYear, referenceMonth, 1, 8, 0, 0);

        DateTime usageBaseDate = new DateTime(referenceYear, referenceMonth, 1).AddMonths(-1);

        if (!definition.usePreviousMonthWorkedHours)
            usageBaseDate = new DateTime(referenceYear, referenceMonth, 1);

        return AddExpenseInternal(
            definition.title,
            definition.description,
            definition.origin,
            amount,
            dueDate,
            FinancePaymentStatus.Open,
            true,
            definition.recurringId,
            referenceYear,
            referenceMonth,
            usageBaseDate.Year,
            usageBaseDate.Month,
            workedMinutesUsedInCalculation,
            createdAt,
            true
        );
    }

    public FinanceMovementData RegisterShopPurchaseExpense(
        string title,
        string description,
        int amount,
        FinanceMovementOrigin origin = FinanceMovementOrigin.ProductPurchase,
        bool spendMoneyNow = true)
    {
        amount = Mathf.Max(0, amount);

        if (amount <= 0)
            return null;

        if (string.IsNullOrWhiteSpace(title))
            title = "Compra na loja";

        if (spendMoneyNow)
        {
            bool spent = SpendMoneyInternal(amount);

            if (!spent)
            {
                Debug.LogWarning("[FinanceManager] Dinheiro insuficiente para registrar compra da loja.");
                return null;
            }
        }

        DateTime now = GetNow();

        FinanceMovementData movement = AddExpenseInternal(
            title,
            description,
            origin,
            amount,
            now,
            FinancePaymentStatus.Paid,
            false,
            string.Empty,
            0,
            0,
            0,
            0,
            0,
            now,
            false
        );

        NotifyChanged();

        if (logMessagesInConsole)
            Debug.Log($"[FinanceManager] Compra registrada: -R$ {amount} | Caixa atual: R$ {currentCash}");

        return movement;
    }

    public bool HasAutoExpenseForMonth(string recurringSourceId, int year, int month)
    {
        if (string.IsNullOrWhiteSpace(recurringSourceId))
            return false;

        return movements.Any(x =>
            x.isAutoGenerated &&
            x.recurringSourceId == recurringSourceId &&
            x.referenceYear == year &&
            x.referenceMonth == month);
    }

    public bool PayExpense(string movementId)
    {
        if (string.IsNullOrWhiteSpace(movementId))
            return false;

        FinanceMovementData movement = movements.FirstOrDefault(x => x.id == movementId);

        if (movement == null)
            return false;

        DateTime now = GetNow();

        if (!movement.IsExpense || movement.IsPaid)
            return false;

        if (!movement.IsPayable(now))
            return false;

        bool paid = SpendMoneyInternal(movement.amount);

        if (!paid)
            return false;

        movement.paymentStatus = FinancePaymentStatus.Paid;

        SaveData();
        NotifyChanged();

        if (logMessagesInConsole)
            Debug.Log($"[FinanceManager] Despesa paga: -R$ {movement.amount} | Caixa atual: R$ {currentCash}");

        return true;
    }

    public int GetCurrentCash()
    {
        return currentCash;
    }

    public int GetOpenDebtTotal()
    {
        return movements
            .Where(x => x.IsExpense && !x.IsPaid)
            .Sum(x => x.amount);
    }

    public int GetOverdueDebtTotal()
    {
        DateTime now = GetNow();

        return movements
            .Where(x => x.IsOverdue(now))
            .Sum(x => x.amount);
    }

    public int GetCurrentMonthDebtTotal()
    {
        DateTime now = GetNow();

        return movements
            .Where(x =>
                x.IsExpense &&
                !x.IsPaid &&
                x.HasDueDate &&
                x.DueDate.Year == now.Year &&
                x.DueDate.Month == now.Month)
            .Sum(x => x.amount);
    }

    public void RefreshFinanceUI()
    {
        if (caixaAtualText != null)
        {
            caixaAtualText.text = $"{moneyPrefix}{currentCash}";

            if (useDynamicColor)
                caixaAtualText.color = currentCash >= 0 ? positiveColor : negativeColor;
        }

        if (dividaAtualText != null)
        {
            int divida = GetOverdueDebtTotal();

            dividaAtualText.text = $"{moneyPrefix}{divida}";

            if (useDynamicColor)
                dividaAtualText.color = divida > 0 ? negativeColor : positiveColor;
        }
    }

    public List<FinanceMonthGroupData> GetStatementMonthGroups()
    {
        return movements
            .OrderByDescending(GetOrderDate)
            .GroupBy(x => new
            {
                Year = GetReferenceDate(x).Year,
                Month = GetReferenceDate(x).Month
            })
            .Select(g => new FinanceMonthGroupData(
                g.Key.Year,
                g.Key.Month,
                g.OrderByDescending(GetOrderDate).ToList()
            ))
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ToList();
    }

    public void ClearHistory()
    {
        movements.Clear();
        SaveData();
        NotifyChanged();

        if (logMessagesInConsole)
            Debug.Log("[FinanceManager] Histórico financeiro apagado.");
    }

    public void ClearAllData(bool resetCashToZero = false)
    {
        movements.Clear();
        SaveData();

        if (resetCashToZero)
        {
            currentCash = 0;
            SaveCash();
        }

        NotifyChanged();

        if (logMessagesInConsole)
            Debug.Log("[FinanceManager] Todos os dados financeiros foram apagados.");
    }

    private FinanceMovementData AddExpenseInternal(
        string title,
        string description,
        FinanceMovementOrigin origin,
        int amount,
        DateTime dueDate,
        FinancePaymentStatus paymentStatus,
        bool isAutoGenerated,
        string recurringSourceId,
        int referenceYear,
        int referenceMonth,
        int usageSourceYear,
        int usageSourceMonth,
        int usageWorkedMinutes,
        DateTime? createdAtOverride,
        bool notify)
    {
        amount = Mathf.Max(0, amount);

        if (string.IsNullOrWhiteSpace(title))
            title = "Despesa";

        if (amount <= 0)
            return null;

        DateTime now = createdAtOverride ?? GetNow();

        FinanceMovementData movement = new FinanceMovementData
        {
            id = Guid.NewGuid().ToString("N"),
            movementType = FinanceMovementType.Expense,
            origin = origin,
            title = title,
            description = description,
            amount = amount,
            paymentStatus = paymentStatus,
            CreatedAt = now,
            DueDate = dueDate,
            isAutoGenerated = isAutoGenerated,
            recurringSourceId = recurringSourceId,
            referenceYear = referenceYear,
            referenceMonth = referenceMonth,
            usageSourceYear = usageSourceYear,
            usageSourceMonth = usageSourceMonth,
            usageWorkedMinutes = usageWorkedMinutes
        };

        movements.Add(movement);
        SortMovements();
        SaveData();

        if (notify)
            NotifyChanged();

        return movement;
    }

    private void AddMoneyInternal(int amount)
    {
        if (amount <= 0)
            return;

        currentCash += amount;
        SaveCash();
    }

    private bool SpendMoneyInternal(int amount)
    {
        if (amount <= 0)
            return true;

        if (currentCash < amount)
            return false;

        currentCash -= amount;
        SaveCash();
        return true;
    }

    private void TryMigrateLegacyCash()
    {
        if (!migrateLegacyMoney || !usePlayerPrefs)
            return;

        if (PlayerPrefs.GetInt(legacyMigrationDoneKey, 0) == 1)
            return;

        bool hasNewCash = PlayerPrefs.HasKey(currentCashSaveKey);

        int migratedValue = 0;
        bool foundLegacy = false;

        if (PlayerPrefs.HasKey(legacyPlayerMoneyKey))
        {
            migratedValue = Mathf.Max(migratedValue, PlayerPrefs.GetInt(legacyPlayerMoneyKey, 0));
            foundLegacy = true;
        }

        if (PlayerPrefs.HasKey(legacyCashRegisterKey))
        {
            migratedValue = Mathf.Max(migratedValue, PlayerPrefs.GetInt(legacyCashRegisterKey, 0));
            foundLegacy = true;
        }

        if (!hasNewCash)
        {
            if (foundLegacy)
            {
                PlayerPrefs.SetInt(currentCashSaveKey, Mathf.Max(0, migratedValue));

                if (logMessagesInConsole)
                    Debug.Log($"[FinanceManager] Saldo legado migrado para a nova chave: R$ {migratedValue}");
            }
            else
            {
                PlayerPrefs.SetInt(currentCashSaveKey, Mathf.Max(0, defaultStartingCash));
            }
        }

        PlayerPrefs.DeleteKey(legacyPlayerMoneyKey);
        PlayerPrefs.DeleteKey(legacyCashRegisterKey);
        PlayerPrefs.SetInt(legacyMigrationDoneKey, 1);
        PlayerPrefs.Save();
    }

    private DateTime GetReferenceDate(FinanceMovementData movement)
    {
        if (movement.IsExpense && movement.HasDueDate)
            return movement.DueDate;

        return movement.CreatedAt;
    }

    private DateTime GetOrderDate(FinanceMovementData movement)
    {
        if (movement.IsExpense && movement.HasDueDate)
            return movement.DueDate;

        return movement.CreatedAt;
    }

    private void SortMovements()
    {
        movements.Sort((a, b) => GetOrderDate(b).CompareTo(GetOrderDate(a)));
    }

    private void SaveCash()
    {
        if (!usePlayerPrefs)
            return;

        PlayerPrefs.SetInt(currentCashSaveKey, currentCash);
        PlayerPrefs.Save();
    }

    private void LoadCash()
    {
        if (!usePlayerPrefs)
        {
            currentCash = Mathf.Max(0, defaultStartingCash);
            return;
        }

        if (PlayerPrefs.HasKey(currentCashSaveKey))
        {
            currentCash = Mathf.Max(0, PlayerPrefs.GetInt(currentCashSaveKey, defaultStartingCash));
            return;
        }

        currentCash = Mathf.Max(0, defaultStartingCash);
        SaveCash();
    }

    private void SaveData()
    {
        if (!usePlayerPrefs)
            return;

        FinanceMovementSaveWrapper wrapper = new FinanceMovementSaveWrapper
        {
            items = movements.ToArray()
        };

        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(historySaveKey, json);
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        movements.Clear();

        if (!usePlayerPrefs)
            return;

        if (!PlayerPrefs.HasKey(historySaveKey))
            return;

        string json = PlayerPrefs.GetString(historySaveKey, string.Empty);

        if (string.IsNullOrWhiteSpace(json))
            return;

        FinanceMovementSaveWrapper wrapper = JsonUtility.FromJson<FinanceMovementSaveWrapper>(json);

        if (wrapper == null || wrapper.items == null)
            return;

        movements.AddRange(wrapper.items);
        SortMovements();
    }

    private void NotifyChanged()
    {
        RefreshFinanceUI();

        OnCashChanged?.Invoke(currentCash);
        OnFinanceDataChanged?.Invoke();
    }
}

[Serializable]
public class FinanceMonthGroupData
{
    public int Year;
    public int Month;
    public List<FinanceMovementData> Movements;

    public FinanceMonthGroupData(int year, int month, List<FinanceMovementData> movements)
    {
        Year = year;
        Month = month;
        Movements = movements;
    }

    public string GetMonthYearText()
    {
        string[] months =
        {
            "",
            "Janeiro",
            "Fevereiro",
            "Março",
            "Abril",
            "Maio",
            "Junho",
            "Julho",
            "Agosto",
            "Setembro",
            "Outubro",
            "Novembro",
            "Dezembro"
        };

        if (Month < 1 || Month > 12)
            return $"{Month} / {Year}";

        return $"{months[Month]} / {Year}";
    }
}