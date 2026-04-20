using System.Collections.Generic;

public static class ServiceToolCompatibility
{
    private static readonly Dictionary<ServiceActionType, ProductCategory[]> CompatibilityMap = new Dictionary<ServiceActionType, ProductCategory[]>()
    {
        { ServiceActionType.Wash, new[] { ProductCategory.ProdutoCapilar } },
        { ServiceActionType.Comb, new[] { ProductCategory.Pente } },
        { ServiceActionType.Cut, new[] { ProductCategory.MaquinaDeCorte, ProductCategory.Tesoura } },
        { ServiceActionType.Razor, new[] { ProductCategory.Navalha, ProductCategory.Laminas } },
        { ServiceActionType.Finish, new[] { ProductCategory.ProdutoCapilar, ProductCategory.Pente } },
        { ServiceActionType.Define, new[] { ProductCategory.ProdutoCapilar, ProductCategory.Pente } },
        { ServiceActionType.Beard, new[] { ProductCategory.Navalha, ProductCategory.MaquinaDeCorte, ProductCategory.Tesoura } },
        { ServiceActionType.Finalize, new[] { ProductCategory.ProdutoCapilar, ProductCategory.Pente } }
    };

    public static bool IsCompatible(ServiceActionType action, ProductCategory category)
    {
        if (!CompatibilityMap.TryGetValue(action, out ProductCategory[] categories))
            return false;

        for (int i = 0; i < categories.Length; i++)
        {
            if (categories[i] == category)
                return true;
        }

        return false;
    }
}
