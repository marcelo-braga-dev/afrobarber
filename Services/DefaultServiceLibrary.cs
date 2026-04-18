using System.Collections.Generic;

public static class DefaultServiceLibrary
{
    public static List<ClientRequestData> CreateDefaultServices()
    {
        return new List<ClientRequestData>
        {
            new ClientRequestData
            {
                id = "degrade_simples",
                requestName = "Degradê Simples",
                description = "Laterais baixas com transição suave.",
                haircutType = HaircutType.Degrade,
                serviceTime = 8f,
                servicePrice = 25,
                difficulty = 1,
                requiredItems = new List<ServiceRequirementData>
                {
                    new ServiceRequirementData
                    {
                        requirementId = "maq_corte",
                        displayName = "Máquina de Corte",
                        category = ProductCategory.MaquinaDeCorte,
                        usageType = InventoryUsageType.PorHoraDeUso,
                        hoursConsumed = 1
                    },
                    new ServiceRequirementData
                    {
                        requirementId = "pente",
                        displayName = "Pente",
                        category = ProductCategory.Pente,
                        usageType = InventoryUsageType.PorHoraDeUso,
                        hoursConsumed = 1
                    },
                    new ServiceRequirementData
                    {
                        requirementId = "navalha",
                        displayName = "Navalha",
                        category = ProductCategory.Navalha,
                        usageType = InventoryUsageType.PorHoraDeUso,
                        hoursConsumed = 1
                    },
                    new ServiceRequirementData
                    {
                        requirementId = "lamina",
                        displayName = "Lâmina",
                        specificProductId = "lamina_padrao",
                        category = ProductCategory.Navalha,
                        usageType = InventoryUsageType.PorServico,
                        amountConsumed = 1
                    }
                }
            },

            new ClientRequestData
            {
                id = "americano",
                requestName = "Americano",
                description = "Topo alto com laterais alinhadas.",
                haircutType = HaircutType.Americano,
                serviceTime = 10f,
                servicePrice = 35,
                difficulty = 2,
                requiredItems = new List<ServiceRequirementData>
                {
                    new ServiceRequirementData
                    {
                        requirementId = "maq_corte",
                        displayName = "Máquina de Corte",
                        category = ProductCategory.MaquinaDeCorte,
                        usageType = InventoryUsageType.PorHoraDeUso,
                        hoursConsumed = 1
                    },
                    new ServiceRequirementData
                    {
                        requirementId = "tesoura",
                        displayName = "Tesoura",
                        category = ProductCategory.Tesoura,
                        usageType = InventoryUsageType.PorHoraDeUso,
                        hoursConsumed = 1
                    },
                    new ServiceRequirementData
                    {
                        requirementId = "pente",
                        displayName = "Pente",
                        category = ProductCategory.Pente,
                        usageType = InventoryUsageType.PorHoraDeUso,
                        hoursConsumed = 1
                    }
                }
            },

            new ClientRequestData
            {
                id = "black_power",
                requestName = "Black Power Alinhado",
                description = "Volume alinhado com acabamento detalhado.",
                haircutType = HaircutType.BlackPower,
                serviceTime = 12f,
                servicePrice = 45,
                difficulty = 3,
                requiredItems = new List<ServiceRequirementData>
                {
                    new ServiceRequirementData
                    {
                        requirementId = "tesoura",
                        displayName = "Tesoura",
                        category = ProductCategory.Tesoura,
                        usageType = InventoryUsageType.PorHoraDeUso,
                        hoursConsumed = 1
                    },
                    new ServiceRequirementData
                    {
                        requirementId = "maq_corte",
                        displayName = "Máquina de Corte",
                        category = ProductCategory.MaquinaDeCorte,
                        usageType = InventoryUsageType.PorHoraDeUso,
                        hoursConsumed = 1
                    },
                    new ServiceRequirementData
                    {
                        requirementId = "navalha",
                        displayName = "Navalha",
                        category = ProductCategory.Navalha,
                        usageType = InventoryUsageType.PorHoraDeUso,
                        hoursConsumed = 1
                    },
                    new ServiceRequirementData
                    {
                        requirementId = "lamina",
                        displayName = "Lâmina",
                        specificProductId = "lamina_padrao",
                        category = ProductCategory.Navalha,
                        usageType = InventoryUsageType.PorServico,
                        amountConsumed = 1
                    },
                    new ServiceRequirementData
                    {
                        requirementId = "creme",
                        displayName = "Creme",
                        category = ProductCategory.ProdutoCapilar,
                        usageType = InventoryUsageType.PorServico,
                        amountConsumed = 2
                    }
                }
            },

            new ClientRequestData
            {
                id = "barba_completa",
                requestName = "Barba Completa",
                description = "Contorno e ajuste completo da barba.",
                haircutType = HaircutType.RiscoNavalhado,
                serviceTime = 7f,
                servicePrice = 20,
                difficulty = 1,
                requiredItems = new List<ServiceRequirementData>
                {
                    new ServiceRequirementData
                    {
                        requirementId = "navalha",
                        displayName = "Navalha",
                        category = ProductCategory.Navalha,
                        usageType = InventoryUsageType.PorHoraDeUso,
                        hoursConsumed = 1
                    },
                    new ServiceRequirementData
                    {
                        requirementId = "lamina",
                        displayName = "Lâmina",
                        specificProductId = "lamina_padrao",
                        category = ProductCategory.Navalha,
                        usageType = InventoryUsageType.PorServico,
                        amountConsumed = 1
                    },
                    new ServiceRequirementData
                    {
                        requirementId = "creme",
                        displayName = "Creme",
                        category = ProductCategory.ProdutoCapilar,
                        usageType = InventoryUsageType.PorServico,
                        amountConsumed = 1
                    }
                }
            }
        };
    }
}