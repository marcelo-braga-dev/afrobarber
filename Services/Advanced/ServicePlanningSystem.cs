using System.Collections.Generic;
using UnityEngine;

public class ServicePlanningSystem : MonoBehaviour
{
    [SerializeField] private List<ServiceActionDefinition> actionDefinitions = new List<ServiceActionDefinition>
    {
        new ServiceActionDefinition { actionType = ServiceActionType.Wash, baseMinutes = 2f },
        new ServiceActionDefinition { actionType = ServiceActionType.Comb, baseMinutes = 1.2f },
        new ServiceActionDefinition { actionType = ServiceActionType.Cut, baseMinutes = 5f },
        new ServiceActionDefinition { actionType = ServiceActionType.Razor, baseMinutes = 3f },
        new ServiceActionDefinition { actionType = ServiceActionType.Finish, baseMinutes = 1.5f },
        new ServiceActionDefinition { actionType = ServiceActionType.Define, baseMinutes = 2f },
        new ServiceActionDefinition { actionType = ServiceActionType.Beard, baseMinutes = 2.8f },
        new ServiceActionDefinition { actionType = ServiceActionType.Finalize, baseMinutes = 1f }
    };

    public ServicePlanData CreatePlan(string serviceId)
    {
        return new ServicePlanData { serviceId = serviceId };
    }

    public void AddStep(ServicePlanData plan, ServiceActionType actionType, ProductInventoryState selectedTool)
    {
        if (plan == null)
            return;

        ServiceActionPlanStep step = new ServiceActionPlanStep
        {
            order = plan.steps.Count,
            actionType = actionType,
            productUniqueId = selectedTool != null ? selectedTool.uniqueId : string.Empty,
            productId = selectedTool != null ? selectedTool.productId : string.Empty
        };

        step.estimatedMinutes = EstimateStepDuration(step);
        plan.steps.Add(step);
    }

    public void RemoveStep(ServicePlanData plan, int index)
    {
        if (plan == null || index < 0 || index >= plan.steps.Count)
            return;

        plan.steps.RemoveAt(index);
        ReindexAndRecalculate(plan);
    }

    public void ReorderStep(ServicePlanData plan, int from, int to)
    {
        if (plan == null || from < 0 || from >= plan.steps.Count || to < 0 || to >= plan.steps.Count)
            return;

        ServiceActionPlanStep item = plan.steps[from];
        plan.steps.RemoveAt(from);
        plan.steps.Insert(to, item);
        ReindexAndRecalculate(plan);
    }

    public void ReplaceTool(ServicePlanData plan, int index, ProductInventoryState selectedTool)
    {
        if (plan == null || index < 0 || index >= plan.steps.Count)
            return;

        ServiceActionPlanStep step = plan.steps[index];
        step.productUniqueId = selectedTool != null ? selectedTool.uniqueId : string.Empty;
        step.productId = selectedTool != null ? selectedTool.productId : string.Empty;
        step.estimatedMinutes = EstimateStepDuration(step);
    }

    public float EstimateStepDuration(ServiceActionPlanStep step)
    {
        float baseMinutes = 2f;

        ServiceActionDefinition def = actionDefinitions.Find(x => x.actionType == step.actionType);
        if (def != null)
            baseMinutes = Mathf.Max(0.5f, def.baseMinutes);

        float toolMultiplier = GetToolTimeMultiplier(step.productUniqueId);
        return Mathf.Max(0.3f, baseMinutes * toolMultiplier);
    }

    private float GetToolTimeMultiplier(string productUniqueId)
    {
        if (string.IsNullOrWhiteSpace(productUniqueId) || InventoryManager.Instance == null)
            return 1.15f;

        ProductInventoryState state = InventoryManager.Instance.GetItemByUniqueId(productUniqueId);
        if (state == null)
            return 1.15f;

        ProductData data = InventoryManager.Instance.GetProductDataById(state.productId);
        if (data == null)
            return 1.15f;

        float quality = (data.precisao + data.velocidade + data.durabilidade) / 300f;
        float normalized = Mathf.Clamp01(quality);
        return Mathf.Lerp(1.2f, 0.75f, normalized);
    }

    private void ReindexAndRecalculate(ServicePlanData plan)
    {
        for (int i = 0; i < plan.steps.Count; i++)
        {
            plan.steps[i].order = i;
            plan.steps[i].estimatedMinutes = EstimateStepDuration(plan.steps[i]);
        }
    }
}
