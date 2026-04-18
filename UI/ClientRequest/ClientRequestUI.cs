using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientRequestUI : MonoBehaviour
{
    public static ClientRequestUI Instance { get; private set; }

    public bool IsOpen => rootPanel != null && rootPanel.activeSelf;

    [Header("Janela principal")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private Image requestIcon;
    [SerializeField] private TMP_Text requestNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text xpText;
    [SerializeField] private TMP_Text difficultyText;
    [SerializeField] private TMP_Text summaryText;
    [SerializeField] private Image readinessFillImage;
    [SerializeField] private TMP_Text readinessText;
    [SerializeField] private TMP_Text missingItemsWarningText;

    [Header("Preview educacional (opcional)")]
    [SerializeField] private GameObject educationPanel;
    [SerializeField] private TMP_Text educationTitleText;
    [SerializeField] private TMP_Text educationSummaryText;
    [SerializeField] private CutEducationPreviewUI educationPreviewUI;

    [Header("Itens requeridos")]
    [SerializeField] private Transform requirementsParent;
    [SerializeField] private RequestRequirementSlotUI requirementSlotPrefab;

    [Header("Botões")]
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button dispenseButton;
    [SerializeField] private Button closeButton;

    [Header("Seleção sobreposta")]
    [SerializeField] private GameObject overlayPanel;
    [SerializeField] private Transform overlayContentParent;
    [SerializeField] private ProductChoiceItemUI productChoicePrefab;
    [SerializeField] private Button overlayCloseButton;
    [SerializeField] private TMP_Text overlayTitleText;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private ClientNPC currentClient;
    private ClientRequestData currentRequest;
    private PreparedServiceLoadout currentLoadout;

    private readonly List<GameObject> spawnedRequirementSlots = new List<GameObject>();
    private readonly List<GameObject> spawnedChoiceItems = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            HideImmediate();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Show(ClientNPC client)
    {
        if (client == null)
            return;

        ClientRequestData request = GetClientRequest(client);

        if (request == null)
        {
            Debug.LogWarning("[ClientRequestUI] O cliente não possui RequestData.");
            return;
        }

        request.SyncCompatibilityFields();

        currentClient = client;
        currentRequest = request;
        currentLoadout = ServiceLoadoutBuilder.BuildDefaultLoadout(currentRequest);

        if (rootPanel != null)
            rootPanel.SetActive(true);

        if (overlayPanel != null)
            overlayPanel.SetActive(false);

        BindButtons();
        Refresh();

        if (enableDebugLogs)
            Debug.Log($"[ClientRequestUI] Show() aberto para {client.name} | Pedido: {currentRequest.requestName}");
    }

    private ClientRequestData GetClientRequest(ClientNPC client)
    {
        if (client == null)
            return null;

        if (client.RequestData != null)
            return client.RequestData;

        if (client.CurrentRequest != null)
            return client.CurrentRequest;

        return null;
    }

    private void BindButtons()
    {
        if (acceptButton != null)
        {
            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(OnClickAccept);
        }

        if (dispenseButton != null)
        {
            dispenseButton.onClick.RemoveAllListeners();
            dispenseButton.onClick.AddListener(OnClickDispense);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }

        if (overlayCloseButton != null)
        {
            overlayCloseButton.onClick.RemoveAllListeners();
            overlayCloseButton.onClick.AddListener(CloseOverlay);
        }
    }

    private void Refresh()
    {
        if (currentRequest == null)
            return;

        currentRequest.SyncCompatibilityFields();

        if (requestIcon != null)
            requestIcon.sprite = currentRequest.icon;

        if (requestNameText != null)
            requestNameText.text = currentRequest.requestName;

        if (descriptionText != null)
            descriptionText.text = currentRequest.GetDescription();

        if (priceText != null)
            priceText.text = $"R$ {currentRequest.ServicePrice}";

        if (timeText != null)
            timeText.text = $"{currentRequest.ServiceTimeRoundedMinutes} min";

        if (difficultyText != null)
            difficultyText.text = $"Dificuldade {currentRequest.difficulty}";

        RefreshEducationalPanel();
        BuildRequirementSlots();
        UpdateSummaryAndReadiness();

        bool hasAllItems = InventoryManager.Instance != null &&
                           InventoryManager.Instance.HasAllRequirements(currentRequest.requiredItems) &&
                           ServiceLoadoutBuilder.IsLoadoutComplete(currentRequest, currentLoadout);

        if (missingItemsWarningText != null)
        {
            missingItemsWarningText.gameObject.SetActive(!hasAllItems);
            missingItemsWarningText.text = "Você não possui todos os itens necessários para realizar este atendimento.";
        }

        if (acceptButton != null)
            acceptButton.gameObject.SetActive(hasAllItems);

        if (dispenseButton != null)
            dispenseButton.gameObject.SetActive(!hasAllItems);
    }

    private void RefreshEducationalPanel()
    {
        AfroCutInfo cutInfo = null;

        if (currentClient != null)
            cutInfo = currentClient.GetCurrentCutInfo();

        bool hasEducation = currentRequest != null &&
                            (currentRequest.HasEducationalContent() || cutInfo != null);

        if (educationPanel != null)
            educationPanel.SetActive(hasEducation);

        if (!hasEducation)
        {
            if (educationPreviewUI != null)
                educationPreviewUI.Clear();
            return;
        }

        if (educationTitleText != null)
        {
            if (!string.IsNullOrWhiteSpace(currentRequest.educationalTitle))
                educationTitleText.text = currentRequest.educationalTitle;
            else if (cutInfo != null)
                educationTitleText.text = cutInfo.cutName;
            else
                educationTitleText.text = "Contexto cultural";
        }

        if (educationSummaryText != null)
        {
            if (!string.IsNullOrWhiteSpace(currentRequest.educationalSummary))
                educationSummaryText.text = currentRequest.educationalSummary;
            else if (cutInfo != null)
                educationSummaryText.text = cutInfo.historicalSummary;
            else
                educationSummaryText.text = string.Empty;
        }

        if (educationPreviewUI != null && cutInfo != null)
            educationPreviewUI.SetData(cutInfo);
    }

    private void BuildRequirementSlots()
    {
        ClearRequirementSlots();

        if (currentRequest.requiredItems == null || requirementSlotPrefab == null || requirementsParent == null)
            return;

        foreach (ServiceRequirementData requirement in currentRequest.requiredItems)
        {
            if (requirement == null)
                continue;

            RequestRequirementSlotUI slot = Instantiate(requirementSlotPrefab, requirementsParent);
            spawnedRequirementSlots.Add(slot.gameObject);

            PreparedServiceItemSelection selection = currentLoadout.GetSelectionByRequirement(requirement.requirementId);
            ProductInventoryState selectedState = selection != null && InventoryManager.Instance != null
                ? InventoryManager.Instance.GetItemByUniqueId(selection.productUniqueId)
                : null;

            ProductData selectedProduct = selectedState != null && InventoryManager.Instance != null
                ? InventoryManager.Instance.GetProductDataById(selectedState.productId)
                : null;

            bool available = InventoryManager.Instance != null &&
                             InventoryManager.Instance.HasUsableItemForRequirement(requirement);

            slot.Setup(currentRequest, requirement, selectedProduct, available, this);
        }
    }

    private void UpdateSummaryAndReadiness()
    {
        if (currentRequest == null || currentRequest.requiredItems == null || currentRequest.requiredItems.Count == 0)
        {
            if (summaryText != null)
                summaryText.text = "Nenhum item exigido.";

            if (readinessText != null)
                readinessText.text = "100%";

            if (readinessFillImage != null)
                readinessFillImage.fillAmount = 1f;

            return;
        }

        int total = 0;
        int ready = 0;

        foreach (ServiceRequirementData requirement in currentRequest.requiredItems)
        {
            if (requirement == null)
                continue;

            total++;

            PreparedServiceItemSelection selection = currentLoadout.GetSelectionByRequirement(requirement.requirementId);
            if (selection == null)
                continue;

            if (InventoryManager.Instance == null)
                continue;

            ProductInventoryState state = InventoryManager.Instance.GetItemByUniqueId(selection.productUniqueId);
            if (state == null)
                continue;

            ProductData product = InventoryManager.Instance.GetProductDataById(state.productId);
            if (product == null)
                continue;

            if (state.IsUsable(product))
                ready++;
        }

        float percent = total <= 0 ? 1f : (float)ready / total;

        if (summaryText != null)
        {
            if (ready == total)
                summaryText.text = "Tudo pronto para iniciar o atendimento.";
            else
                summaryText.text = "Faltam itens necessários para esse atendimento.";
        }

        if (readinessText != null)
            readinessText.text = $"{Mathf.RoundToInt(percent * 100f)}%";

        if (readinessFillImage != null)
            readinessFillImage.fillAmount = percent;
    }

    public void OpenSelectionForRequirement(ClientRequestData request, ServiceRequirementData requirement)
    {
        if (request == null || requirement == null)
            return;

        if (overlayPanel != null)
            overlayPanel.SetActive(true);

        if (overlayTitleText != null)
            overlayTitleText.text = $"Escolher: {requirement.GetDisplayName()}";

        ClearChoiceItems();

        if (InventoryManager.Instance == null || productChoicePrefab == null || overlayContentParent == null)
            return;

        List<ProductInventoryState> options = InventoryManager.Instance.GetUsableItemsForRequirement(requirement);

        foreach (ProductInventoryState option in options)
        {
            ProductData product = InventoryManager.Instance.GetProductDataById(option.productId);
            if (product == null)
                continue;

            ProductChoiceItemUI choice = Instantiate(productChoicePrefab, overlayContentParent);
            spawnedChoiceItems.Add(choice.gameObject);

            choice.Setup(product, option, () =>
            {
                SelectRequirementItem(requirement, option);
            });
        }
    }

    public void OpenSelectionForRequirement(ServiceRequirementData requirement)
    {
        OpenSelectionForRequirement(currentRequest, requirement);
    }

    private void SelectRequirementItem(ServiceRequirementData requirement, ProductInventoryState option)
    {
        PreparedServiceItemSelection existing = currentLoadout.GetSelectionByRequirement(requirement.requirementId);

        if (existing == null)
        {
            existing = new PreparedServiceItemSelection
            {
                requirementId = requirement.requirementId
            };

            currentLoadout.selections.Add(existing);
        }

        existing.productUniqueId = option.uniqueId;
        existing.productId = option.productId;

        if (ServiceSelectionMemory.Instance != null)
            ServiceSelectionMemory.Instance.SaveLastProductId(currentRequest.id, requirement.requirementId, option.productId);

        CloseOverlay();
        Refresh();
    }

    private void OnClickAccept()
    {
        if (currentClient == null || currentRequest == null)
            return;

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[ClientRequestUI] InventoryManager.Instance não encontrado.");
            return;
        }

        if (!InventoryManager.Instance.HasAllRequirements(currentRequest.requiredItems))
        {
            Refresh();
            return;
        }

        if (!ServiceLoadoutBuilder.IsLoadoutComplete(currentRequest, currentLoadout))
        {
            Refresh();
            return;
        }

        currentClient.SetPreparedLoadout(currentLoadout);
        currentClient.CallForService();
        Hide();
    }

    private void OnClickDispense()
    {
        if (currentClient != null)
            currentClient.DispenseDueToMissingItems();

        Hide();
    }

    private void CloseOverlay()
    {
        if (overlayPanel != null)
            overlayPanel.SetActive(false);

        ClearChoiceItems();
    }

    public void Hide()
    {
        currentClient = null;
        currentRequest = null;
        currentLoadout = null;

        CloseOverlay();
        HideImmediate();
    }

    private void HideImmediate()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);

        if (overlayPanel != null)
            overlayPanel.SetActive(false);

        if (educationPreviewUI != null)
            educationPreviewUI.Clear();

        ClearRequirementSlots();
        ClearChoiceItems();
    }

    private void ClearRequirementSlots()
    {
        foreach (GameObject obj in spawnedRequirementSlots)
        {
            if (obj != null)
                Destroy(obj);
        }

        spawnedRequirementSlots.Clear();
    }

    private void ClearChoiceItems()
    {
        foreach (GameObject obj in spawnedChoiceItems)
        {
            if (obj != null)
                Destroy(obj);
        }

        spawnedChoiceItems.Clear();
    }
}