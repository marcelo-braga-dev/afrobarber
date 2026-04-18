using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FinanceUIController : MonoBehaviour
{
    public static FinanceUIController Instance { get; private set; }

    [Header("Janela")]
    [SerializeField] private GameObject root;

    [Header("Resumo")]
    [SerializeField] private TMP_Text currentCashText;
    [SerializeField] private TMP_Text openDebtText;
    [SerializeField] private TMP_Text overdueDebtText;

    [Header("Extrato")]
    [SerializeField] private Transform statementContainer;
    [SerializeField] private FinanceMonthCardUI monthCardPrefab;

    [Header("Botões")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button refreshButton;

    [Header("Visual")]
    [SerializeField] private string moneyPrefix = "R$ ";

    private readonly List<FinanceMonthCardUI> spawnedCards = new List<FinanceMonthCardUI>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        HideImmediate();
    }

    private void OnEnable()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Hide);
            closeButton.onClick.AddListener(Hide);
        }

        if (refreshButton != null)
        {
            refreshButton.onClick.RemoveListener(RefreshUI);
            refreshButton.onClick.AddListener(RefreshUI);
        }

        if (FinanceManager.Instance != null)
        {
            FinanceManager.Instance.OnFinanceDataChanged -= RefreshUI;
            FinanceManager.Instance.OnFinanceDataChanged += RefreshUI;
        }

        RefreshUI();
    }

    private void OnDisable()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Hide);

        if (refreshButton != null)
            refreshButton.onClick.RemoveListener(RefreshUI);

        if (FinanceManager.Instance != null)
            FinanceManager.Instance.OnFinanceDataChanged -= RefreshUI;
    }

    public void Show()
    {
        if (root != null)
            root.SetActive(true);
        else
            gameObject.SetActive(true);

        RefreshUI();
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    public void HideImmediate()
    {
        if (root != null)
            root.SetActive(false);
        else
            gameObject.SetActive(false);
    }

    public void Toggle()
    {
        GameObject target = root != null ? root : gameObject;
        bool next = !target.activeSelf;
        target.SetActive(next);

        if (next)
            RefreshUI();
    }

    public void RefreshUI()
    {
        if (FinanceManager.Instance == null)
            return;

        UpdateSummary();
        RebuildStatement();
    }

    private void UpdateSummary()
    {
        int currentCash = FinanceManager.Instance.GetCurrentCash();
        int openDebt = FinanceManager.Instance.GetCurrentMonthDebtTotal();
        int overdueDebt = FinanceManager.Instance.GetOverdueDebtTotal();

        if (currentCashText != null)
            currentCashText.text = $"{moneyPrefix}{currentCash}";

        if (openDebtText != null)
            openDebtText.text = $"{moneyPrefix}{openDebt}";

        if (overdueDebtText != null)
            overdueDebtText.text = $"{moneyPrefix}{overdueDebt}";
    }

    private void RebuildStatement()
    {
        ClearCards();

        if (statementContainer == null || monthCardPrefab == null)
            return;

        List<FinanceMonthGroupData> groups = FinanceManager.Instance.GetStatementMonthGroups();

        foreach (FinanceMonthGroupData group in groups)
        {
            FinanceMonthCardUI card = Instantiate(monthCardPrefab, statementContainer);
            card.Setup(group);
            spawnedCards.Add(card);
        }
    }

    private void ClearCards()
    {
        for (int i = 0; i < spawnedCards.Count; i++)
        {
            if (spawnedCards[i] != null)
                Destroy(spawnedCards[i].gameObject);
        }

        spawnedCards.Clear();
    }
}