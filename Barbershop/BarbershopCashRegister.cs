using UnityEngine;
using UnityEngine.Events;

public class BarbershopCashRegister : BootstrapUIBehaviour
{
    public static BarbershopCashRegister Instance { get; private set; }

    [Header("Eventos")]
    public UnityEvent<int> OnMoneyChanged;

    [Header("Debug")]
    [SerializeField] private bool logMessagesInConsole = false;

    private bool subscribed;

    public int CurrentMoney
    {
        get
        {
            if (FinanceManager.Instance == null)
                return 0;

            return FinanceManager.Instance.CurrentCash;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    protected override void OnBootstrapInitialize()
    {
        SubscribeToFinance();
        EmitCurrentMoney();
    }

    private void OnDisable()
    {
        UnsubscribeFromFinance();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        UnsubscribeFromFinance();
    }

    public void AddMoney(int amount)
    {
        if (FinanceManager.Instance == null)
        {
            LogWarning("FinanceManager.Instance não encontrado.");
            return;
        }

        FinanceManager.Instance.AddMoney(amount, "Entrada via caixa da barbearia");
    }

    public bool TrySpendMoney(int amount)
    {
        if (FinanceManager.Instance == null)
        {
            LogWarning("FinanceManager.Instance não encontrado.");
            return false;
        }

        return FinanceManager.Instance.SpendMoney(amount, "Saída via caixa da barbearia");
    }

    public void SetMoney(int amount)
    {
        if (FinanceManager.Instance == null)
        {
            LogWarning("FinanceManager.Instance não encontrado.");
            return;
        }

        FinanceManager.Instance.SetCurrentCash(amount);
    }

    private void SubscribeToFinance()
    {
        if (subscribed)
            return;

        if (FinanceManager.Instance == null)
        {
            LogWarning("FinanceManager.Instance não encontrado ao tentar inscrever.");
            return;
        }

        FinanceManager.Instance.OnCashChanged -= HandleCashChanged;
        FinanceManager.Instance.OnCashChanged += HandleCashChanged;

        subscribed = true;
    }

    private void UnsubscribeFromFinance()
    {
        if (!subscribed)
            return;

        if (FinanceManager.Instance != null)
            FinanceManager.Instance.OnCashChanged -= HandleCashChanged;

        subscribed = false;
    }

    private void HandleCashChanged(int amount)
    {
        OnMoneyChanged?.Invoke(amount);

        if (logMessagesInConsole)
            Debug.Log($"[BarbershopCashRegister] Valor atualizado: R$ {amount}");
    }

    private void EmitCurrentMoney()
    {
        HandleCashChanged(CurrentMoney);
    }

    private void LogWarning(string message)
    {
        if (logMessagesInConsole)
            Debug.LogWarning("[BarbershopCashRegister] " + message);
    }
}