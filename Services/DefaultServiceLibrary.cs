using System.Collections.Generic;
using UnityEngine;

public static class DefaultServiceLibrary
{
    public static List<ClientRequestData> CreateDefaultServices()
    {
        return new List<ClientRequestData>
        {
            CreateRequest(
                "degrade_simples",
                "Degradê Simples",
                "Laterais baixas com transição suave.",
                HaircutType.Degrade,
                8f,
                25,
                1,
                new List<ServiceRequirementData>
                {
                    CreateRequirement("maq_corte", "Máquina de Corte", ProductCategory.MaquinaDeCorte, InventoryUsageType.PorHoraDeUso, 0, 1),
                    CreateRequirement("pente", "Pente", ProductCategory.Pente, InventoryUsageType.PorHoraDeUso, 0, 1),
                    CreateRequirement("navalha", "Navalha", ProductCategory.Navalha, InventoryUsageType.PorHoraDeUso, 0, 1),
                    CreateRequirement("lamina", "Lâmina", ProductCategory.Navalha, InventoryUsageType.PorServico, 1, 0, "lamina_padrao")
                }),

            CreateRequest(
                "americano",
                "Americano",
                "Topo alto com laterais alinhadas.",
                HaircutType.Americano,
                10f,
                35,
                2,
                new List<ServiceRequirementData>
                {
                    CreateRequirement("maq_corte", "Máquina de Corte", ProductCategory.MaquinaDeCorte, InventoryUsageType.PorHoraDeUso, 0, 1),
                    CreateRequirement("tesoura", "Tesoura", ProductCategory.Tesoura, InventoryUsageType.PorHoraDeUso, 0, 1),
                    CreateRequirement("pente", "Pente", ProductCategory.Pente, InventoryUsageType.PorHoraDeUso, 0, 1)
                }),

            CreateRequest(
                "black_power",
                "Black Power Alinhado",
                "Volume alinhado com acabamento detalhado.",
                HaircutType.BlackPower,
                12f,
                45,
                3,
                new List<ServiceRequirementData>
                {
                    CreateRequirement("tesoura", "Tesoura", ProductCategory.Tesoura, InventoryUsageType.PorHoraDeUso, 0, 1),
                    CreateRequirement("maq_corte", "Máquina de Corte", ProductCategory.MaquinaDeCorte, InventoryUsageType.PorHoraDeUso, 0, 1),
                    CreateRequirement("navalha", "Navalha", ProductCategory.Navalha, InventoryUsageType.PorHoraDeUso, 0, 1),
                    CreateRequirement("lamina", "Lâmina", ProductCategory.Navalha, InventoryUsageType.PorServico, 1, 0, "lamina_padrao"),
                    CreateRequirement("creme", "Creme", ProductCategory.ProdutoCapilar, InventoryUsageType.PorServico, 2)
                }),

            CreateRequest(
                "barba_completa",
                "Barba Completa",
                "Contorno e ajuste completo da barba.",
                HaircutType.RiscoNavalhado,
                7f,
                20,
                1,
                new List<ServiceRequirementData>
                {
                    CreateRequirement("navalha", "Navalha", ProductCategory.Navalha, InventoryUsageType.PorHoraDeUso, 0, 1),
                    CreateRequirement("lamina", "Lâmina", ProductCategory.Navalha, InventoryUsageType.PorServico, 1, 0, "lamina_padrao"),
                    CreateRequirement("creme", "Creme", ProductCategory.ProdutoCapilar, InventoryUsageType.PorServico, 1)
                })
        };
    }

    private static ClientRequestData CreateRequest(
        string id,
        string requestName,
        string description,
        HaircutType haircutType,
        float serviceTime,
        int servicePrice,
        int difficulty,
        List<ServiceRequirementData> requiredItems)
    {
        ClientRequestData request = ScriptableObject.CreateInstance<ClientRequestData>();

        request.hideFlags = HideFlags.HideAndDontSave;

        request.id = id;
        request.requestId = id;
        request.requestName = requestName;
        request.description = description;
        request.haircutType = haircutType;
        request.serviceTime = serviceTime;
        request.servicePrice = servicePrice;
        request.difficulty = Mathf.Max(1, difficulty);
        request.requiredItems = requiredItems ?? new List<ServiceRequirementData>();

        request.SyncCompatibilityFields();

        return request;
    }

    private static ServiceRequirementData CreateRequirement(
        string requirementId,
        string displayName,
        ProductCategory category,
        InventoryUsageType usageType,
        int amountConsumed,
        int hoursConsumed = 0,
        string specificProductId = "")
    {
        return new ServiceRequirementData
        {
            requirementId = requirementId,
            displayName = displayName,
            category = category,
            usageType = usageType,
            amountConsumed = Mathf.Max(0, amountConsumed),
            hoursConsumed = Mathf.Max(0, hoursConsumed),
            specificProductId = specificProductId
        };
    }
}