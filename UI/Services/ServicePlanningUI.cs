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

    private ClientNPC currentClient;
    private ServicePlanData currentPlan;

    private System.Action<ClientNPC> onStartRequested;

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

        Hide();
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
    }

    private void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }

    private void RefreshHeader()
    {
        ClientRequestData request = currentClient != null ? currentClient.RequestData : null;

        if (clientNameText != null)
            clientNameText.text = currentClient != null ? currentClient.ClientDisplayName : "Cliente";

        if (serviceNameText != null)
            serviceNameText.text = request != null ? request.RequestName : "Serviço";

        if (serviceDescriptionText != null)
            serviceDescriptionText.text = request != null ? request.GetDescription() : "";

        if (idealTimeText != null)
            idealTimeText.text = request != null ? $"Tempo ideal: {request.ServiceTime:0.#} min" : "Tempo ideal: -";

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
    }

    private void RefreshPlanList()
    {
        ClearChildren(planContent);

        if (planContent == null || stepItemPrefab == null || currentPlan == null)
        {
            RefreshEstimatedTime();
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
    }

    private void MoveStepDown(int index)
    {
        if (currentPlan == null || planningSystem == null)
            return;

        planningSystem.ReorderStep(currentPlan, index, index + 1);
        RefreshPlanList();
    }

    private void RefreshEstimatedTime()
    {
        float total = currentPlan != null ? currentPlan.GetEstimatedTotalMinutes() : 0f;

        if (estimatedTimeText != null)
            estimatedTimeText.text = $"Tempo estimado: {total:0.#} min";
    }

    private bool ValidatePlan()
    {
        string warning = "";

        if (currentPlan == null || currentPlan.steps.Count == 0)
        {
            warning = "Monte uma sequência de ações antes de iniciar.";
        }
        else if (requireFinalizeStep && !HasAction(ServiceActionType.Finalize))
        {
            warning = "Adicione a ação Finalizar para concluir o atendimento.";
        }

        if (warningText != null)
            warningText.text = warning;

        if (startButton != null)
            startButton.interactable = string.IsNullOrWhiteSpace(warning);

        return string.IsNullOrWhiteSpace(warning);
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

    private void ClearChildren(Transform parent)
    {
        if (parent == null)
            return;

        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
    }
}