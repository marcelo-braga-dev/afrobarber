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
    [SerializeField] private ScrollRect statementScrollRect;
    [SerializeField] private Transform statementContainer;
    [SerializeField] private FinanceMonthCardUI monthCardPrefab;

    [Header("Paginação / Performance")]
    [SerializeField] private int monthCardsPerPage = 1;

    [Tooltip("Quando chegar perto do fim do scroll, carrega mais linhas ou mais meses. 0.15 = faltando 15% para o fim.")]
    [Range(0.01f, 0.5f)]
    [SerializeField] private float loadMoreThreshold = 0.15f;

    [SerializeField] private float loadCooldown = 0.25f;

    [Header("Estado vazio")]
    [SerializeField] private TMP_Text emptyStatementText;

    [Header("Botões")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button refreshButton;

    [Header("Visual")]
    [SerializeField] private string moneyPrefix = "R$ ";

    private readonly List<FinanceMonthCardUI> spawnedCards = new List<FinanceMonthCardUI>();
    private readonly List<FinanceMonthGroupData> cachedGroups = new List<FinanceMonthGroupData>();

    private int currentLoadedMonthIndex;
    private bool isLoadingMore;
    private float lastLoadTime;

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

    private void Update()
    {
        TryAutoLoadMoreByScroll();
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
        RebuildStatementPaged();
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

    private void RebuildStatementPaged()
    {
        ClearCards();
        cachedGroups.Clear();

        if (statementContainer == null || monthCardPrefab == null)
            return;

        List<FinanceMonthGroupData> groups = FinanceManager.Instance.GetStatementMonthGroups();

        if (groups != null)
            cachedGroups.AddRange(groups);

        currentLoadedMonthIndex = 0;
        isLoadingMore = false;
        lastLoadTime = 0f;

        if (emptyStatementText != null)
            emptyStatementText.gameObject.SetActive(cachedGroups.Count == 0);

        LoadNextMonthPage();

        if (statementScrollRect != null)
            statementScrollRect.verticalNormalizedPosition = 1f;
    }

    private void TryAutoLoadMoreByScroll()
    {
        if (!IsOpen())
            return;

        if (statementScrollRect == null)
            return;

        if (isLoadingMore)
            return;

        if (Time.unscaledTime - lastLoadTime < loadCooldown)
            return;

        bool isNearBottom = statementScrollRect.verticalNormalizedPosition <= loadMoreThreshold;

        if (!isNearBottom)
            return;

        LoadMoreFromCurrentStatement();
    }

    private void LoadMoreFromCurrentStatement()
    {
        isLoadingMore = true;
        lastLoadTime = Time.unscaledTime;

        if (TryLoadMoreRowsFromLastCard())
        {
            isLoadingMore = false;
            RebuildContentLayout();
            return;
        }

        LoadNextMonthPage();

        isLoadingMore = false;
        RebuildContentLayout();
    }

    private bool TryLoadMoreRowsFromLastCard()
    {
        if (spawnedCards.Count <= 0)
            return false;

        FinanceMonthCardUI lastCard = spawnedCards[spawnedCards.Count - 1];

        if (lastCard == null)
            return false;

        return lastCard.TryLoadMoreRows();
    }

    private void LoadNextMonthPage()
    {
        if (!HasMoreMonthsToLoad())
            return;

        if (statementContainer == null || monthCardPrefab == null)
            return;

        int safeMonthCardsPerPage = Mathf.Max(1, monthCardsPerPage);
        int endIndex = Mathf.Min(currentLoadedMonthIndex + safeMonthCardsPerPage, cachedGroups.Count);

        for (int i = currentLoadedMonthIndex; i < endIndex; i++)
        {
            FinanceMonthGroupData group = cachedGroups[i];

            FinanceMonthCardUI card = Instantiate(monthCardPrefab, statementContainer);
            card.Setup(group);
            spawnedCards.Add(card);
        }

        currentLoadedMonthIndex = endIndex;
    }

    private bool HasMoreMonthsToLoad()
    {
        return currentLoadedMonthIndex < cachedGroups.Count;
    }

    private bool IsOpen()
    {
        if (root != null)
            return root.activeInHierarchy;

        return gameObject.activeInHierarchy;
    }

    private void RebuildContentLayout()
    {
        RectTransform contentRect = statementContainer as RectTransform;

        if (contentRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
    }

    private void ClearCards()
    {
        for (int i = spawnedCards.Count - 1; i >= 0; i--)
        {
            if (spawnedCards[i] != null)
                Destroy(spawnedCards[i].gameObject);
        }

        spawnedCards.Clear();
    }
}