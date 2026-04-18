using System;
using UnityEngine;

[Serializable]
public class ProductInventoryState
{
    public string uniqueId;
    public string productId;

    public int currentAmount;
    public int maxAmount;

    public bool isBroken;
    public bool isEmpty;

    public ProductInventoryState(ProductData product)
    {
        uniqueId = Guid.NewGuid().ToString();
        productId = product.productId;
        maxAmount = product.GetCapacidadeInicial();
        currentAmount = maxAmount;
        isBroken = false;
        isEmpty = false;
    }

    public float GetNormalized()
    {
        if (maxAmount <= 0)
            return 0f;

        return Mathf.Clamp01((float)currentAmount / maxAmount);
    }

    public bool IsUsable(ProductData product)
    {
        if (product == null)
            return false;

        switch (product.inventoryItemType)
        {
            case InventoryItemType.Duravel:
                return !isBroken && currentAmount > 0;

            case InventoryItemType.Consumivel:
            case InventoryItemType.CaixaComUnidades:
                return !isEmpty && currentAmount > 0;

            default:
                return false;
        }
    }

    public void Consume(ProductData product, int amount)
    {
        if (product == null || amount <= 0)
            return;

        currentAmount -= amount;

        if (currentAmount < 0)
            currentAmount = 0;

        switch (product.inventoryItemType)
        {
            case InventoryItemType.Duravel:
                if (currentAmount <= 0)
                {
                    currentAmount = 0;
                    isBroken = true;
                }
                break;

            case InventoryItemType.Consumivel:
            case InventoryItemType.CaixaComUnidades:
                if (currentAmount <= 0)
                {
                    currentAmount = 0;
                    isEmpty = true;
                }
                break;
        }
    }

    public string GetStatusText(ProductData product)
    {
        if (product == null)
            return "Inválido";

        switch (product.inventoryItemType)
        {
            case InventoryItemType.Duravel:
                return isBroken ? "Quebrado" : "Disponível";

            case InventoryItemType.Consumivel:
                return isEmpty ? "Esgotado" : "Disponível";

            case InventoryItemType.CaixaComUnidades:
                return isEmpty ? "Vazio" : "Disponível";

            default:
                return "Desconhecido";
        }
    }

    public string GetRemainingText(ProductData product)
    {
        if (product == null)
            return "-";

        switch (product.inventoryItemType)
        {
            case InventoryItemType.Duravel:
                return $"{currentAmount}h";

            case InventoryItemType.Consumivel:
                return $"{currentAmount}/{maxAmount}";

            case InventoryItemType.CaixaComUnidades:
                return $"{currentAmount}un";

            default:
                return "-";
        }
    }

    public string GetEstimatedLifeText(ProductData product)
    {
        if (product == null)
            return "-";

        switch (product.inventoryItemType)
        {
            case InventoryItemType.Duravel:
                return $"Vida útil estimada: {maxAmount}h";

            case InventoryItemType.Consumivel:
                if (product.usageType == InventoryUsageType.PorHoraDeUso)
                    return $"Estimativa: {maxAmount}h de uso";

                return $"Estimativa: {Mathf.Max(1, maxAmount / Mathf.Max(1, product.consumoPorUso))} serviços";

            case InventoryItemType.CaixaComUnidades:
                return $"Estimativa: {maxAmount} usos";

            default:
                return "-";
        }
    }
}