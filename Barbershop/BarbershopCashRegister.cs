using UnityEngine;
using UnityEngine.Events;

public class BarbershopCashRegister : MonoBehaviour
{
    public static BarbershopCashRegister Instance { get; private set; }

    [Header("Eventos")]
    public UnityEvent<int> OnMoneyChanged;

    [Header("Debug")]
    [SerializeField] private bool logMessagesInConsole = true;

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

    private void OnEnable()
    {
        SubscribeToFinance();
        EmitCurrentMoney();
    }

    private void Start()
    {
        EmitCurrentMoney();
    }

    private void OnDisable()
    {
        UnsubscribeFromFinance();
    }

    public void AddMoney(int amount)
    {
        if (FinanceManager.Instance == null)
        {
            Debug.LogWarning("[BarbershopCashRegister] FinanceManager.Instance não encontrado.");
            return;
        }

        FinanceManager.Instance.AddMoney(amount, "Entrada via caixa da barbearia");
    }

    public bool TrySpendMoney(int amount)
    {
        if (FinanceManager.Instance == null)
        {
            Debug.LogWarning("[BarbershopCashRegister] FinanceManager.Instance não encontrado.");
            return false;
        }

        return FinanceManager.Instance.SpendMoney(amount, "Saída via caixa da barbearia");
    }

    public void SetMoney(int amount)
    {
        if (FinanceManager.Instance == null)
        {
            Debug.LogWarning("[BarbershopCashRegister] FinanceManager.Instance não encontrado.");
            return;
        }

        FinanceManager.Instance.SetCurrentCash(amount);
    }

    private void SubscribeToFinance()
    {
        if (FinanceManager.Instance != null)
        {
            FinanceManager.Instance.OnCashChanged -= HandleCashChanged;
            FinanceManager.Instance.OnCashChanged += HandleCashChanged;
        }
    }

    private void UnsubscribeFromFinance()
    {
        if (FinanceManager.Instance != null)
        {
            FinanceManager.Instance.OnCashChanged -= HandleCashChanged;
        }
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
}