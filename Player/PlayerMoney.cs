using System;
using TMPro;
using UnityEngine;

public class PlayerMoney : MonoBehaviour
{
    public static PlayerMoney Instance { get; private set; }

    [Header("Configuração")]
    [SerializeField] private int initialMoney = 0;
    [SerializeField] private bool usePlayerPrefs = true;
    [SerializeField] private string saveKey = "AFROBARBER_PLAYER_MONEY";

    [Header("UI")]
    [SerializeField] private TMP_Text caixaAtualText;
    [SerializeField] private TMP_Text dividaAtualText;
    [SerializeField] private string moneyPrefix = "R$ ";

    [Header("Visual")]
    [SerializeField] private bool useDynamicColor = true;
    [SerializeField] private Color positiveColor = new Color(0.2f, 0.9f, 0.2f);
    [SerializeField] private Color negativeColor = new Color(0.9f, 0.2f, 0.2f);

    private int currentMoney;

    public int CurrentMoney => currentMoney;

    public event Action<int> OnMoneyChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadMoney();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateMoneyUI();
        NotifyMoneyChanged();

        if (FinanceManager.Instance != null)
        {
            FinanceManager.Instance.OnFinanceDataChanged += UpdateMoneyUI;
        }
    }

    private void OnDestroy()
    {
        if (FinanceManager.Instance != null)
        {
            FinanceManager.Instance.OnFinanceDataChanged -= UpdateMoneyUI;
        }
    }

    // =========================
    // SALVAMENTO
    // =========================

    private void LoadMoney()
    {
        if (usePlayerPrefs)
            currentMoney = PlayerPrefs.GetInt(saveKey, initialMoney);
        else
            currentMoney = initialMoney;
    }

    private void SaveMoney()
    {
        if (!usePlayerPrefs)
            return;

        PlayerPrefs.SetInt(saveKey, currentMoney);
        PlayerPrefs.Save();
    }

    // =========================
    // UI
    // =========================

    public void UpdateMoneyUI()
    {
        // SALDO
        if (caixaAtualText != null)
        {
            caixaAtualText.text = $"{moneyPrefix}{currentMoney}";

            if (useDynamicColor)
                caixaAtualText.color = currentMoney >= 0 ? positiveColor : negativeColor;
        }

        // DÍVIDA
        if (dividaAtualText != null && FinanceManager.Instance != null)
        {
            int divida = FinanceManager.Instance.GetOverdueDebtTotal();

            dividaAtualText.text = $"{moneyPrefix}{divida}";

            if (useDynamicColor)
                dividaAtualText.color = divida > 0 ? negativeColor : positiveColor;
        }
    }

    // =========================
    // OPERAÇÕES DE DINHEIRO
    // =========================

    public void AddMoney(int amount)
    {
        if (amount <= 0)
            return;

        currentMoney += amount;

        SaveMoney();
        UpdateMoneyUI();
        NotifyMoneyChanged();

        Debug.Log($"[PlayerMoney] Dinheiro adicionado: {amount}. Saldo atual: {currentMoney}");
    }

    public bool HasEnoughMoney(int amount)
    {
        return currentMoney >= amount;
    }

    public bool SpendMoney(int amount)
    {
        if (amount <= 0)
            return false;

        if (currentMoney < amount)
            return false;

        currentMoney -= amount;

        SaveMoney();
        UpdateMoneyUI();
        NotifyMoneyChanged();

        Debug.Log($"[PlayerMoney] Dinheiro gasto: {amount}. Saldo atual: {currentMoney}");
        return true;
    }

    // =========================
    // EVENTOS
    // =========================

    private void NotifyMoneyChanged()
    {
        OnMoneyChanged?.Invoke(currentMoney);
    }
}