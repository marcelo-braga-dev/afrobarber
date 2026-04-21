using System.Collections.Generic;
using UnityEngine;

public class AdvancedServiceWorkflowManager : MonoBehaviour
{
    [SerializeField] private bool enableAdvancedWorkflow = true;
    [SerializeField] private ServicePlanningSystem planningSystem;
    [SerializeField] private ServiceExecutionSystem executionSystem;

    private readonly Dictionary<ClientNPC, ServicePlanData> plansByClient = new Dictionary<ClientNPC, ServicePlanData>();

    public bool EnableAdvancedWorkflow => enableAdvancedWorkflow;
    public ServicePlanningSystem PlanningSystem => planningSystem;

    private void Awake()
    {
        if (planningSystem == null)
            planningSystem = GetComponent<ServicePlanningSystem>();

        if (executionSystem == null)
            executionSystem = GetComponent<ServiceExecutionSystem>();

        if (executionSystem != null)
            executionSystem.OnActionCompleted += HandleActionCompleted;
    }

    private void OnDestroy()
    {
        if (executionSystem != null)
            executionSystem.OnActionCompleted -= HandleActionCompleted;
    }

    public bool TryPreparePlan(ClientNPC client)
    {
        if (!enableAdvancedWorkflow || client == null || client.RequestData == null || planningSystem == null)
            return false;

        ServicePlanData plan = planningSystem.CreatePlan(client.RequestData.RequestId);

        AddIfCompatible(plan, ServiceActionType.Wash);
        AddIfCompatible(plan, ServiceActionType.Cut);
        AddIfCompatible(plan, ServiceActionType.Finish);
        AddIfCompatible(plan, ServiceActionType.Finalize);

        plansByClient[client] = plan;

        NPCIdentity identity = client.GetComponent<NPCIdentity>();
        GlobalDialogueManager.Instance?.AddNpcMessage(identity, "Beleza, pode montar meu atendimento.", DialogueContextType.Service);

        return true;
    }

    public ServicePlanData CreateEmptyPlanForClient(ClientNPC client)
    {
        if (!enableAdvancedWorkflow || client == null || client.RequestData == null || planningSystem == null)
            return null;

        ServicePlanData plan = planningSystem.CreatePlan(client.RequestData.RequestId);
        plansByClient[client] = plan;

        return plan;
    }

    public void SetPlanForClient(ClientNPC client, ServicePlanData plan)
    {
        if (client == null || plan == null)
            return;

        plansByClient[client] = plan;
    }

    public bool HasValidPlan(ClientNPC client)
    {
        if (client == null)
            return false;

        return plansByClient.TryGetValue(client, out ServicePlanData plan)
            && plan != null
            && plan.steps != null
            && plan.steps.Count > 0;
    }

    public ServicePlanData GetPlanForClient(ClientNPC client)
    {
        if (client == null)
            return null;

        plansByClient.TryGetValue(client, out ServicePlanData plan);
        return plan;
    }

    public bool TryExecutePlan(ClientNPC client, System.Action<AdvancedServiceResult> onFinished)
    {
        if (!enableAdvancedWorkflow || client == null || executionSystem == null)
            return false;

        if (!plansByClient.TryGetValue(client, out ServicePlanData plan) || plan == null || plan.steps.Count == 0)
            return false;

        ClientRequestData request = client.RequestData;

        executionSystem.StartExecution(plan, client, (actionResults, actualMinutes) =>
        {
            AdvancedServiceResult result = AdvancedServiceOutcomeResolver.Resolve(client, request, plan, actionResults, actualMinutes);
            ApplyResult(client, request, result);
            onFinished?.Invoke(result);
        });

        return true;
    }

    private void AddIfCompatible(ServicePlanData plan, ServiceActionType action)
    {
        ProductInventoryState tool = FindBestToolForAction(action);
        planningSystem.AddStep(plan, action, tool);
    }

    public ProductInventoryState FindBestToolForActionPublic(ServiceActionType action)
    {
        return FindBestToolForAction(action);
    }

    public List<ProductInventoryState> GetCompatibleToolsForAction(ServiceActionType action)
    {
        List<ProductInventoryState> compatibleTools = new List<ProductInventoryState>();

        if (InventoryManager.Instance == null)
            return compatibleTools;

        List<ProductInventoryState> all = InventoryManager.Instance.GetAllOwnedItems();

        for (int i = 0; i < all.Count; i++)
        {
            ProductInventoryState item = all[i];

            if (item == null)
                continue;

            ProductData product = InventoryManager.Instance.GetProductDataById(item.productId);

            if (product == null || !item.IsUsable(product))
                continue;

            if (!ServiceToolCompatibility.IsCompatible(action, product.category))
                continue;

            compatibleTools.Add(item);
        }

        return compatibleTools;
    }

    private ProductInventoryState FindBestToolForAction(ServiceActionType action)
    {
        if (InventoryManager.Instance == null)
            return null;

        List<ProductInventoryState> all = InventoryManager.Instance.GetAllOwnedItems();

        ProductInventoryState best = null;
        float bestScore = float.MinValue;

        for (int i = 0; i < all.Count; i++)
        {
            ProductInventoryState item = all[i];
            ProductData product = InventoryManager.Instance.GetProductDataById(item.productId);

            if (product == null || !item.IsUsable(product))
                continue;

            if (!ServiceToolCompatibility.IsCompatible(action, product.category))
                continue;

            float score = item.GetNormalized() + ((product.precisao + product.velocidade + product.durabilidade) / 300f);

            if (score > bestScore)
            {
                bestScore = score;
                best = item;
            }
        }

        return best;
    }

    private void ApplyResult(ClientNPC client, ClientRequestData request, AdvancedServiceResult result)
    {
        if (result == null)
            return;

        PlayerWallet wallet = Object.FindFirstObjectByType<PlayerWallet>();

        if (wallet != null)
            wallet.AddMoney(result.moneyReward);

        if (PlayerXPManager.Instance != null)
            PlayerXPManager.Instance.AddXP(result.xpReward);

        NPCIdentity identity = client != null ? client.GetComponent<NPCIdentity>() : null;
        NPCRelationshipMemory memory = client != null ? client.GetComponent<NPCRelationshipMemory>() : null;

        if (identity != null)
            identity.AddRelationship(Mathf.Lerp(-8f, 8f, result.finalScore / 5f));

        if (memory != null)
        {
            memory.AddMemory(
                "service_result",
                $"Serviço {request?.RequestName} finalizado com nota {result.finalRating} ({result.finalScore:0.0}).",
                Mathf.Lerp(-5f, 5f, result.finalScore / 5f)
            );
        }

        GlobalDialogueManager.Instance?.AddSystemMessage(
            $"Atendimento finalizado: {result.finalRating} | +R$ {result.moneyReward} | +XP {result.xpReward}",
            DialogueContextType.Service
        );
    }

    private void HandleActionCompleted(ServiceActionExecutionResult actionResult)
    {
        if (actionResult == null)
            return;

        GlobalDialogueManager.Instance?.AddSystemMessage(
            $"Etapa {actionResult.step.actionType}: {actionResult.rating} ({actionResult.score:0.0})",
            DialogueContextType.Service
        );
    }
}