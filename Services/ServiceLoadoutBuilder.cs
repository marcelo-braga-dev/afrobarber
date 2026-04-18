using System.Collections.Generic;
using UnityEngine;

public static class ServiceLoadoutBuilder
{
    public static PreparedServiceLoadout BuildDefaultLoadout(ClientRequestData request)
    {
        PreparedServiceLoadout loadout = new PreparedServiceLoadout();

        if (request == null)
            return loadout;

        loadout.serviceId = request.id;

        if (request.requiredItems == null)
            return loadout;

        foreach (ServiceRequirementData requirement in request.requiredItems)
        {
            if (requirement == null) continue;

            List<ProductInventoryState> options = InventoryManager.Instance.GetUsableItemsForRequirement(requirement);
            if (options.Count == 0)
                continue;

            ProductInventoryState selected = null;

            string rememberedProductId = ServiceSelectionMemory.Instance != null
                ? ServiceSelectionMemory.Instance.GetLastProductId(request.id, requirement.requirementId)
                : "";

            if (!string.IsNullOrWhiteSpace(rememberedProductId))
                selected = options.Find(x => x.productId == rememberedProductId);

            if (selected == null)
            {
                float bestScore = -1f;

                foreach (ProductInventoryState option in options)
                {
                    ProductData product = InventoryManager.Instance.GetProductDataById(option.productId);
                    if (product == null) continue;

                    float score = option.GetNormalized() * 100f + product.precisao + product.velocidade + product.durabilidade;
                    if (score > bestScore)
                    {
                        bestScore = score;
                        selected = option;
                    }
                }
            }

            if (selected == null)
                selected = options[0];

            loadout.selections.Add(new PreparedServiceItemSelection
            {
                requirementId = requirement.requirementId,
                productUniqueId = selected.uniqueId,
                productId = selected.productId
            });
        }

        return loadout;
    }

    public static bool IsLoadoutComplete(ClientRequestData request, PreparedServiceLoadout loadout)
    {
        if (request == null)
            return false;

        if (request.requiredItems == null || request.requiredItems.Count == 0)
            return true;

        if (loadout == null)
            return false;

        foreach (ServiceRequirementData requirement in request.requiredItems)
        {
            if (requirement == null) continue;

            PreparedServiceItemSelection selection = loadout.GetSelectionByRequirement(requirement.requirementId);
            if (selection == null)
                return false;

            ProductInventoryState state = InventoryManager.Instance.GetItemByUniqueId(selection.productUniqueId);
            if (state == null)
                return false;

            ProductData product = InventoryManager.Instance.GetProductDataById(state.productId);
            if (product == null || !state.IsUsable(product))
                return false;
        }

        return true;
    }
}