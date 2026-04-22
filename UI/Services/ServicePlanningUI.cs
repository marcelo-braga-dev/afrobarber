using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServicePlanningUI : MonoBehaviour
{
    [Header("Sistema")]
    [SerializeField] private AdvancedServiceWorkflowManager workflowManager;
    [SerializeField] private ServicePlanningSystem planningSystem;

    [Header("Painel")]
    [SerializeField] private GameObject root;

    [Header("Cabeçalho")]
    [SerializeField] private TMP_Text clientNameText;
    [SerializeField] private TMP_Text serviceNameText;
    [SerializeField] private TMP_Text serviceDescriptionText;
    [SerializeField] private TMP_Text idealTimeText;
    [SerializeField] private TMP_Text estimatedTimeText;
    [SerializeField] private TMP_Text warningText;

    [Header("Cores do tempo estimado")]
    [SerializeField] private Color estimatedTimeOkColor = Color.green;
    [SerializeField] private Color estimatedTimeExceededColor = Color.red;
    [SerializeField] private Color estimatedTimeNeutralColor = Color.white;

    [Header("Mensagens")]
    [SerializeField] private string missingToolWarning = "Existe uma ação sem ferramenta. Clique na imagem do item e selecione uma ferramenta compatível.";
    [SerializeField] private string emptyPlanWarning = "Monte uma sequência de ações antes de iniciar.";
    [SerializeField] private string missingFinalizeWarning = "Adicione a ação Finalizar para concluir o atendimento.";
    [SerializeField] private string actionSelectedHint = "Clique sobre a imagem do item para trocar a ferramenta.";

    [Header("Grid de ações")]
    [SerializeField] private Transform actionsContent;
    [SerializeField] private ServicePlanningActionButton actionButtonPrefab;

    [Header("Lista do plano")]
    [SerializeField] private Transform planContent;
    [SerializeField] private ServicePlanningStepItemUI stepItemPrefab;

    [Header("Botões")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button autoPlanButton;
    [SerializeField] private Button dismissButton;

    [Header("Ações disponíveis")]
    [SerializeField]
    private List<ServiceActionType> availableActions = new List<ServiceActionType>
    {
        ServiceActionType.Wash,
        ServiceActionType.Comb,
        ServiceActionType.Cut,
        ServiceActionType.Razor,
        ServiceActionType.Finish,
        ServiceActionType.Define,
        ServiceActionType.Beard,
        ServiceActionType.Finalize
    };

    [Header("Configuração")]
    [SerializeField] private bool requireFinalizeStep = true;
    [SerializeField] private bool closeWhenStart = true;
    [SerializeField] private bool blockPlayerMovementWhileOpen = true;

    private ClientNPC currentClient;
    private ServicePlanData currentPlan;

    private System.Action<ClientNPC> onStartRequested;
    private bool isBlockingPlayerMovement;

    private void Awake()
    {
        if (workflowManager == null)
            workflowManager = FindFirstObjectByType<AdvancedServiceWorkflowManager>();

        if (planningSystem == null && workflowManager != null)
            planningSystem = workflowManager.PlanningSystem;

        if (planningSystem == null)
            planningSystem = FindFirstObjectByType<ServicePlanningSystem>();

        if (startButton != null)
            startButton.onClick.AddListener(StartPlannedService);

        if (clearButton != null)
            clearButton.onClick.AddListener(ClearPlan);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (autoPlanButton != null)
            autoPlanButton.onClick.AddListener(CreateRecommendedPlan);

        if (dismissButton != null)
            dismissButton.onClick.AddListener(DismissClient);

        Hide();
    }

    private void OnDisable()
    {
        UnblockPlayerMovement();
    }

    private void OnDestroy()
    {
        UnblockPlayerMovement();

        if (startButton != null)
            startButton.onClick.RemoveListener(StartPlannedService);

        if (clearButton != null)
            clearButton.onClick.RemoveListener(ClearPlan);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);

        if (autoPlanButton != null)
            autoPlanButton.onClick.RemoveListener(CreateRecommendedPlan);

        if (dismissButton != null)
            dismissButton.onClick.RemoveListener(DismissClient);
    }

    public void Open(ClientNPC client, System.Action<ClientNPC> startCallback)
    {
        currentClient = client;
        onStartRequested = startCallback;

        if (workflowManager == null)
            workflowManager = FindFirstObjectByType<AdvancedServiceWorkflowManager>();

        if (planningSystem == null && workflowManager != null)
            planningSystem = workflowManager.PlanningSystem;

        if (currentClient == null || currentClient.RequestData == null || workflowManager == null || planningSystem == null)
        {
            Debug.LogWarning("[ServicePlanningUI] Não foi possível abrir planejamento. Cliente, request ou sistemas ausentes.");
            return;
        }

        currentPlan = workflowManager.CreateEmptyPlanForClient(currentClient);

        Show();

        RefreshHeader();
        BuildActionsGrid();
        RefreshPlanList();
    }

    public void Close()
    {
        Hide();
    }

    private void Show()
    {
        if (root != null)
            root.SetActive(true);
        else
            gameObject.SetActive(true);

        BlockPlayerMovement();
    }

    private void Hide()
    {
        if (root != null)
            root.SetActive(false);

        UnblockPlayerMovement();
    }

    private void BlockPlayerMovement()
    {
        if (!blockPlayerMovementWhileOpen)
            return;

        if (isBlockingPlayerMovement)
            return;

        PlayerMovementUIBlocker.Instance?.AddBlock();
        isBlockingPlayerMovement = true;
    }

    private void UnblockPlayerMovement()
    {
        if (!isBlockingPlayerMovement)
            return;

        PlayerMovementUIBlocker.Instance?.RemoveBlock();
        isBlockingPlayerMovement = false;
    }

    private void RefreshHeader()
    {
        ClientRequestData request = currentClient != null ? currentClient.RequestData : null;

        if (clientNameText != null)
            clientNameText.text = currentClient != null ? currentClient.ClientDisplayName : "";

        if (serviceNameText != null)
            serviceNameText.text = request != null ? request.RequestName : "";

        if (serviceDescriptionText != null)
            serviceDescriptionText.text = request != null ? request.GetDescription() : "";

        if (idealTimeText != null)
            idealTimeText.text = request != null ? FormatMinutes(request.ServiceTime) : "";

        RefreshEstimatedTime();
    }

    private void BuildActionsGrid()
    {
        ClearChildren(actionsContent);

        if (actionsContent == null || actionButtonPrefab == null)
            return;

        foreach (ServiceActionType actionType in availableActions)
        {
            ServicePlanningActionButton button = Instantiate(actionButtonPrefab, actionsContent);
            button.Setup(actionType, () => AddAction(actionType));
        }
    }

    private void AddAction(ServiceActionType actionType)
    {
        if (currentPlan == null || planningSystem == null)
            return;

        ProductInventoryState bestTool = workflowManager != null
            ? workflowManager.FindBestToolForActionPublic(actionType)
            : null;

        planningSystem.AddStep(currentPlan, actionType, bestTool);

        RefreshPlanList();

        if (warningText != null && currentPlan != null && currentPlan.steps.Count > 0)
            warningText.text = actionSelectedHint;
    }

    private void ClearPlan()
    {
        if (currentClient == null || workflowManager == null)
            return;

        currentPlan = workflowManager.CreateEmptyPlanForClient(currentClient);

        RefreshPlanList();
    }

    private void CreateRecommendedPlan()
    {
        ClearPlan();

        AddAction(ServiceActionType.Wash);
        AddAction(ServiceActionType.Comb);
        AddAction(ServiceActionType.Cut);
        AddAction(ServiceActionType.Finish);
        AddAction(ServiceActionType.Finalize);

        if (warningText != null)
            warningText.text = actionSelectedHint;
    }

    private void RefreshPlanList()
    {
        ClearChildren(planContent);

        if (planContent == null || stepItemPrefab == null || currentPlan == null)
        {
            RefreshEstimatedTime();
            ValidatePlan();
            return;
        }

        for (int i = 0; i < currentPlan.steps.Count; i++)
        {
            int index = i;
            ServiceActionPlanStep step = currentPlan.steps[index];

            List<ProductInventoryState> compatibleTools = workflowManager != null
                ? workflowManager.GetCompatibleToolsForAction(step.actionType)
                : new List<ProductInventoryState>();

            ServicePlanningStepItemUI item = Instantiate(stepItemPrefab, planContent);

            item.Setup(
                index,
                currentPlan.steps.Count,
                step,
                compatibleTools,
                OnToolChanged,
                RemoveStep,
                MoveStepUp,
                MoveStepDown
            );
        }

        RefreshEstimatedTime();
        ValidatePlan();
    }

    private void OnToolChanged(int index, ProductInventoryState selectedTool)
    {
        if (currentPlan == null || planningSystem == null)
            return;

        planningSystem.ReplaceTool(currentPlan, index, selectedTool);
        RefreshPlanList();

        if (warningText != null)
            warningText.text = actionSelectedHint;
    }

    private void RemoveStep(int index)
    {
        if (currentPlan == null || planningSystem == null)
            return;

        planningSystem.RemoveStep(currentPlan, index);
        RefreshPlanList();
    }

    private void MoveStepUp(int index)
    {
        if (currentPlan == null || planningSystem == null)
            return;

        planningSystem.ReorderStep(currentPlan, index, index - 1);
        RefreshPlanList();

        if (warningText != null && currentPlan != null && currentPlan.steps.Count > 0)
            warningText.text = actionSelectedHint;
    }

    private void MoveStepDown(int index)
    {
        if (currentPlan == null || planningSystem == null)
            return;

        planningSystem.ReorderStep(currentPlan, index, index + 1);
        RefreshPlanList();

        if (warningText != null && currentPlan != null && currentPlan.steps.Count > 0)
            warningText.text = actionSelectedHint;
    }

    private void RefreshEstimatedTime()
    {
        float total = currentPlan != null ? currentPlan.GetEstimatedTotalMinutes() : 0f;

        if (estimatedTimeText != null)
        {
            estimatedTimeText.text = FormatMinutes(total);
            estimatedTimeText.color = GetEstimatedTimeColor(total);
        }
    }

    private Color GetEstimatedTimeColor(float estimatedMinutes)
    {
        ClientRequestData request = currentClient != null ? currentClient.RequestData : null;

        if (request == null)
            return estimatedTimeNeutralColor;

        float idealMinutes = request.ServiceTime;

        if (estimatedMinutes <= 0f)
            return estimatedTimeNeutralColor;

        return estimatedMinutes <= idealMinutes
            ? estimatedTimeOkColor
            : estimatedTimeExceededColor;
    }

    private bool ValidatePlan()
    {
        string blockingWarning = GetBlockingWarning();

        string messageToShow = string.IsNullOrWhiteSpace(blockingWarning)
            ? actionSelectedHint
            : blockingWarning;

        if (warningText != null)
            warningText.text = messageToShow;

        if (startButton != null)
            startButton.interactable = string.IsNullOrWhiteSpace(blockingWarning);

        return string.IsNullOrWhiteSpace(blockingWarning);
    }

    private string GetBlockingWarning()
    {
        if (currentPlan == null || currentPlan.steps == null || currentPlan.steps.Count == 0)
            return emptyPlanWarning;

        if (requireFinalizeStep && !HasAction(ServiceActionType.Finalize))
            return missingFinalizeWarning;

        if (HasAnyStepWithoutValidTool())
            return missingToolWarning;

        return "";
    }

    private bool HasAction(ServiceActionType actionType)
    {
        if (currentPlan == null)
            return false;

        foreach (ServiceActionPlanStep step in currentPlan.steps)
        {
            if (step.actionType == actionType)
                return true;
        }

        return false;
    }

    private void StartPlannedService()
    {
        if (!ValidatePlan())
            return;

        if (currentClient == null || currentPlan == null || workflowManager == null)
            return;

        workflowManager.SetPlanForClient(currentClient, currentPlan);

        if (closeWhenStart)
            Close();

        onStartRequested?.Invoke(currentClient);
    }

    private string FormatMinutes(float value)
    {
        return Mathf.RoundToInt(value).ToString();
    }

    private void ClearChildren(Transform parent)
    {
        if (parent == null)
            return;

        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
    }

    private bool HasAnyStepWithoutValidTool()
    {
        if (currentPlan == null || currentPlan.steps == null)
            return true;

        foreach (ServiceActionPlanStep step in currentPlan.steps)
        {
            if (!HasValidToolForStep(step))
                return true;
        }

        return false;
    }

    private bool HasValidToolForStep(ServiceActionPlanStep step)
    {
        if (step == null)
            return false;

        if (string.IsNullOrWhiteSpace(step.productUniqueId))
            return false;

        if (InventoryManager.Instance == null)
            return false;

        ProductInventoryState item = InventoryManager.Instance.GetItemByUniqueId(step.productUniqueId);

        if (item == null)
            return false;

        ProductData product = InventoryManager.Instance.GetProductDataById(item.productId);

        if (product == null)
            return false;

        if (!item.IsUsable(product))
            return false;

        if (!ServiceToolCompatibility.IsCompatible(step.actionType, product.category))
            return false;

        return true;
    }

    private void DismissClient()
    {
        if (currentClient == null)
        {
            Close();
            return;
        }

        BarbershopServiceManager manager = BarbershopServiceManager.Instance;

        if (manager != null)
        {
            manager.DismissCurrentClientFromPlanning(currentClient);
        }
        else
        {
            Debug.LogWarning("[ServicePlanningUI] BarbershopServiceManager.Instance não encontrado. Não foi possível dispensar o cliente.");
        }

        Close();
    }
}