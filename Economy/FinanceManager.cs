using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FinanceManager : MonoBehaviour
{
    public static FinanceManager Instance { get; private set; }

    [Header("Persistência")]
    [SerializeField] private bool usePlayerPrefs = true;
    [SerializeField] private string saveKey = "AFROBARBER_FINANCE_HISTORY";

    [Header("Migração")]
    [SerializeField] private bool migrateOldCashRegisterKey = true;
    [SerializeField] private string oldCashRegisterKey = "AFROBARBER_CASH_REGISTER_MONEY";

    private readonly List<FinanceMovementData> movements = new List<FinanceMovementData>();

    public IReadOnlyList<FinanceMovementData> Movements => movements;

    public event Action OnFinanceDataChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[FinanceManager] Instância duplicada encontrada. Destruindo apenas este componente.");
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadData();
        TryMigrateOldCashRegisterMoney();
    }

    private void Start()
    {
        NotifyChanged();
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

        AddMoneyToPlayer(amount);

        NotifyChanged();

        Debug.Log($"[FinanceManager] Receita de atendimento registrada: +R$ {amount} | Caixa atual: R$ {GetCurrentCash()}");

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

        AddMoneyToPlayer(amount);

        NotifyChanged();

        Debug.Log($"[FinanceManager] Entrada registrada: +R$ {amount} | Caixa atual: R$ {GetCurrentCash()}");

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
            0
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
            createdAt
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
            bool spent = SpendPlayerMoney(amount);

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
            now
        );

        NotifyChanged();

        Debug.Log($"[FinanceManager] Compra registrada: -R$ {amount} | Caixa atual: R$ {GetCurrentCash()}");

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

        bool paid = SpendPlayerMoney(movement.amount);

        if (!paid)
            return false;

        movement.paymentStatus = FinancePaymentStatus.Paid;
        SaveData();
        NotifyChanged();

        Debug.Log($"[FinanceManager] Despesa paga: -R$ {movement.amount} | Caixa atual: R$ {GetCurrentCash()}");

        return true;
    }

    public int GetCurrentCash()
    {
        if (PlayerMoney.Instance == null)
            return 0;

        return PlayerMoney.Instance.CurrentMoney;
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
        DateTime? createdAtOverride = null)
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
        NotifyChanged();

        return movement;
    }

    private void AddMoneyToPlayer(int amount)
    {
        if (amount <= 0)
            return;

        if (PlayerMoney.Instance == null)
        {
            Debug.LogWarning("[FinanceManager] PlayerMoney.Instance não encontrado. Não foi possível adicionar dinheiro.");
            return;
        }

        PlayerMoney.Instance.AddMoney(amount);
    }

    private bool SpendPlayerMoney(int amount)
    {
        if (amount <= 0)
            return true;

        if (PlayerMoney.Instance == null)
        {
            Debug.LogWarning("[FinanceManager] PlayerMoney.Instance não encontrado. Não foi possível gastar dinheiro.");
            return false;
        }

        return PlayerMoney.Instance.SpendMoney(amount);
    }

    private void TryMigrateOldCashRegisterMoney()
    {
        if (!migrateOldCashRegisterKey)
            return;

        if (!usePlayerPrefs)
            return;

        if (PlayerMoney.Instance == null)
            return;

        if (!PlayerPrefs.HasKey(oldCashRegisterKey))
            return;

        int oldCash = PlayerPrefs.GetInt(oldCashRegisterKey, 0);

        if (oldCash <= 0)
        {
            PlayerPrefs.DeleteKey(oldCashRegisterKey);
            PlayerPrefs.Save();
            return;
        }

        if (PlayerMoney.Instance.CurrentMoney <= 0)
        {
            PlayerMoney.Instance.AddMoney(oldCash);
            Debug.Log($"[FinanceManager] Caixa antigo migrado para PlayerMoney: R$ {oldCash}");
        }

        PlayerPrefs.DeleteKey(oldCashRegisterKey);
        PlayerPrefs.Save();

        NotifyChanged();
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

    private void SaveData()
    {
        if (!usePlayerPrefs)
            return;

        FinanceMovementSaveWrapper wrapper = new FinanceMovementSaveWrapper
        {
            items = movements.ToArray()
        };

        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        movements.Clear();

        if (!usePlayerPrefs)
            return;

        if (!PlayerPrefs.HasKey(saveKey))
            return;

        string json = PlayerPrefs.GetString(saveKey, string.Empty);

        if (string.IsNullOrWhiteSpace(json))
            return;

        FinanceMovementSaveWrapper wrapper = JsonUtility.FromJson<FinanceMovementSaveWrapper>(json);

        if (wrapper == null || wrapper.items == null)
            return;

        movements.AddRange(wrapper.items);
        SortMovements();
    }

    public void ClearAllData()
    {
        movements.Clear();

        if (usePlayerPrefs)
        {
            PlayerPrefs.DeleteKey(saveKey);
            PlayerPrefs.Save();
        }

        NotifyChanged();
    }

    private void NotifyChanged()
    {
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