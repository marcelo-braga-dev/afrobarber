using System.Collections.Generic;
using UnityEngine;

public static class ServiceLoadoutBuilder
{
    public static PreparedServiceLoadout BuildDefaultLoadout(ClientRequestData request)
    {
        PreparedServiceLoadout loadout = new PreparedServiceLoadout();

        if (request == null)
            return loadout;

        loadout.serviceId = request.RequestId;

        if (request.requiredItems == null || request.requiredItems.Count == 0)
            return loadout;

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[ServiceLoadoutBuilder] InventoryManager.Instance não encontrado.");
            return loadout;
        }

        foreach (ServiceRequirementData requirement in request.requiredItems)
        {
            if (requirement == null)
                continue;

            List<ProductInventoryState> options = InventoryManager.Instance.GetUsableItemsForRequirement(requirement);

            if (options == null || options.Count == 0)
                continue;

            ProductInventoryState selected = SelectBestItem(request, requirement, options);

            if (selected == null)
                continue;

            loadout.selections.Add(new PreparedServiceItemSelection
            {
                requirementId = requirement.requirementId,
                productUniqueId = selected.uniqueId,
                productId = selected.productId
            });
        }

        return loadout;
    }

    private static ProductInventoryState SelectBestItem(
        ClientRequestData request,
        ServiceRequirementData requirement,
        List<ProductInventoryState> options
    )
    {
        ProductInventoryState selected = null;

        string rememberedProductId = "";

        if (ServiceSelectionMemory.Instance != null)
            rememberedProductId = ServiceSelectionMemory.Instance.GetLastProductId(request.RequestId, requirement.requirementId);

        if (!string.IsNullOrWhiteSpace(rememberedProductId))
            selected = options.Find(x => x.productId == rememberedProductId);

        if (selected != null)
            return selected;

        float bestScore = -1f;

        foreach (ProductInventoryState option in options)
        {
            if (option == null)
                continue;

            ProductData product = InventoryManager.Instance.GetProductDataById(option.productId);

            if (product == null)
                continue;

            float score = CalculateSelectionScore(product, option);

            if (score > bestScore)
            {
                bestScore = score;
                selected = option;
            }
        }

        if (selected == null && options.Count > 0)
            selected = options[0];

        return selected;
    }

    private static float CalculateSelectionScore(ProductData product, ProductInventoryState itemState)
    {
        if (product == null || itemState == null)
            return 0f;

        float conditionScore = itemState.GetNormalized() * 100f;
        float productScore = product.precisao + product.velocidade + product.durabilidade;

        return conditionScore + productScore;
    }

    public static bool IsLoadoutComplete(ClientRequestData request, PreparedServiceLoadout loadout)
    {
        if (request == null)
            return false;

        if (request.requiredItems == null || request.requiredItems.Count == 0)
            return true;

        if (loadout == null)
            return false;

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[ServiceLoadoutBuilder] InventoryManager.Instance não encontrado.");
            return false;
        }

        foreach (ServiceRequirementData requirement in request.requiredItems)
        {
            if (requirement == null)
                continue;

            if (string.IsNullOrWhiteSpace(requirement.requirementId))
            {
                Debug.LogWarning($"[ServiceLoadoutBuilder] Requisito sem requirementId: {requirement.GetDisplayName()}");
                return false;
            }

            PreparedServiceItemSelection selection = loadout.GetSelectionByRequirement(requirement.requirementId);

            if (selection == null)
                return false;

            ProductInventoryState state = InventoryManager.Instance.GetItemByUniqueId(selection.productUniqueId);

            if (state == null)
                return false;

            ProductData product = InventoryManager.Instance.GetProductDataById(state.productId);

            if (product == null)
                return false;

            if (!state.IsUsable(product))
                return false;

            if (!DoesProductMatchRequirement(product, requirement))
                return false;
        }

        return true;
    }

    private static bool DoesProductMatchRequirement(ProductData product, ServiceRequirementData requirement)
    {
        if (product == null || requirement == null)
            return false;

        if (requirement.RequiresSpecificProduct)
            return product.productId == requirement.specificProductId;

        return product.category == requirement.category;
    }
}