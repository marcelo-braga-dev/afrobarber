using System;
using TMPro;
using UnityEngine;

public class PlayerMoney : MonoBehaviour
{
    public static PlayerMoney Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text caixaAtualText;
    [SerializeField] private TMP_Text dividaAtualText;
    [SerializeField] private string moneyPrefix = "R$ ";

    [Header("Visual")]
    [SerializeField] private bool useDynamicColor = true;
    [SerializeField] private Color positiveColor = new Color(0.2f, 0.9f, 0.2f);
    [SerializeField] private Color negativeColor = new Color(0.9f, 0.2f, 0.2f);

    [Header("Debug")]
    [SerializeField] private bool logWarnings = true;

    public int CurrentMoney
    {
        get
        {
            if (FinanceManager.Instance == null)
            {
                if (logWarnings)
                    Debug.LogWarning("[PlayerMoney] FinanceManager.Instance não encontrado.");
                return 0;
            }

            return FinanceManager.Instance.CurrentCash;
        }
    }

    public event Action<int> OnMoneyChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SubscribeToFinance();
        UpdateMoneyUI();
        NotifyMoneyChanged();
    }

    private void Start()
    {
        SubscribeToFinance();
        UpdateMoneyUI();
        NotifyMoneyChanged();
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

    public void UpdateMoneyUI()
    {
        int currentMoney = CurrentMoney;

        if (caixaAtualText != null)
        {
            caixaAtualText.text = $"{moneyPrefix}{currentMoney}";

            if (useDynamicColor)
                caixaAtualText.color = currentMoney >= 0 ? positiveColor : negativeColor;
        }

        if (dividaAtualText != null && FinanceManager.Instance != null)
        {
            int divida = FinanceManager.Instance.GetOverdueDebtTotal();

            dividaAtualText.text = $"{moneyPrefix}{divida}";

            if (useDynamicColor)
                dividaAtualText.color =  negativeColor;
        }
    }

    public void AddMoney(int amount)
    {
        if (FinanceManager.Instance == null)
        {
            if (logWarnings)
                Debug.LogWarning("[PlayerMoney] FinanceManager.Instance não encontrado.");
            return;
        }

        FinanceManager.Instance.AddMoney(amount, "Entrada via PlayerMoney");
    }

    public bool HasEnoughMoney(int amount)
    {
        if (FinanceManager.Instance == null)
        {
            if (logWarnings)
                Debug.LogWarning("[PlayerMoney] FinanceManager.Instance não encontrado.");
            return false;
        }

        return FinanceManager.Instance.HasEnoughMoney(amount);
    }

    public bool SpendMoney(int amount)
    {
        if (FinanceManager.Instance == null)
        {
            if (logWarnings)
                Debug.LogWarning("[PlayerMoney] FinanceManager.Instance não encontrado.");
            return false;
        }

        return FinanceManager.Instance.SpendMoney(amount, "Saída via PlayerMoney");
    }

    private void SubscribeToFinance()
    {
        if (FinanceManager.Instance == null)
            return;

        FinanceManager.Instance.OnCashChanged -= HandleCashChanged;
        FinanceManager.Instance.OnCashChanged += HandleCashChanged;

        FinanceManager.Instance.OnFinanceDataChanged -= UpdateMoneyUI;
        FinanceManager.Instance.OnFinanceDataChanged += UpdateMoneyUI;
    }

    private void UnsubscribeFromFinance()
    {
        if (FinanceManager.Instance == null)
            return;

        FinanceManager.Instance.OnCashChanged -= HandleCashChanged;
        FinanceManager.Instance.OnFinanceDataChanged -= UpdateMoneyUI;
    }

    private void HandleCashChanged(int currentValue)
    {
        UpdateMoneyUI();
        NotifyMoneyChanged();
    }

    private void NotifyMoneyChanged()
    {
        OnMoneyChanged?.Invoke(CurrentMoney);
    }
}