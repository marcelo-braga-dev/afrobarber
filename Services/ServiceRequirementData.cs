using System;
using UnityEngine;

[Serializable]
public class ServiceRequirementData
{
    public string requirementId;
    public string displayName;
    public ProductCategory category;
    public string specificProductId;
    public InventoryUsageType usageType = InventoryUsageType.PorServico;
    public int hoursConsumed = 1;
    public int amountConsumed = 1;

    public bool RequiresSpecificProduct => !string.IsNullOrWhiteSpace(specificProductId);

    public string GetDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(displayName))
            return displayName;

        if (RequiresSpecificProduct)
            return specificProductId;

        return category.ToString();
    }
}