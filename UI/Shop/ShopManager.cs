using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Bancos por Categoria")]
    [SerializeField] private List<CategoryDatabaseEntry> categoryDatabases = new List<CategoryDatabaseEntry>();

    [Header("UI Produtos")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private ShopItemUI shopItemPrefab;

    [Header("Botões de Categoria")]
    [SerializeField] private List<CategoryButtonUI> categoryButtons = new List<CategoryButtonUI>();

    [Header("Categoria Inicial")]
    [SerializeField] private ProductCategory initialCategory = ProductCategory.MaquinaDeCorte;

    private readonly List<ShopItemUI> instantiatedItems = new List<ShopItemUI>();
    private ProductCategory currentCategory;

    private void Start()
    {
        if (contentParent == null)
        {
            Debug.LogError("ShopManager: contentParent não foi atribuído.", this);
            return;
        }

        if (shopItemPrefab == null)
        {
            Debug.LogError("ShopManager: shopItemPrefab não foi atribuído.", this);
            return;
        }

        SetupCategoryButtons();

        currentCategory = initialCategory;
        GenerateShopByCategory(currentCategory);
        UpdateCategoryButtonsVisual();
    }

    private void SetupCategoryButtons()
    {
        if (categoryButtons == null || categoryButtons.Count == 0)
        {
            Debug.LogWarning("ShopManager: nenhum botão de categoria foi adicionado.", this);
            return;
        }

        foreach (CategoryButtonUI button in categoryButtons)
        {
            if (button == null)
                continue;

            button.Setup(this);
        }
    }

    public void SelectCategory(ProductCategory category)
    {
        currentCategory = category;
        GenerateShopByCategory(currentCategory);
        UpdateCategoryButtonsVisual();
    }

    private void UpdateCategoryButtonsVisual()
    {
        foreach (CategoryButtonUI button in categoryButtons)
        {
            if (button == null)
                continue;

            button.SetSelected(button.Category == currentCategory);
        }
    }

    public void GenerateShopByCategory(ProductCategory category)
    {
        ClearShop();

        ProductDatabase database = GetDatabaseByCategory(category);

        if (database == null)
        {
            Debug.LogWarning($"ShopManager: nenhum banco encontrado para a categoria {category}.", this);
            return;
        }

        if (database.products == null || database.products.Count == 0)
        {
            Debug.LogWarning($"ShopManager: o banco da categoria {category} está vazio.", this);
            return;
        }

        foreach (ProductData product in database.products)
        {
            if (product == null)
                continue;

            if (!product.availableAtStart)
                continue;

            ShopItemUI itemUI = Instantiate(shopItemPrefab, contentParent);
            itemUI.Setup(product, this);
            instantiatedItems.Add(itemUI);
        }
    }

    private ProductDatabase GetDatabaseByCategory(ProductCategory category)
    {
        foreach (CategoryDatabaseEntry entry in categoryDatabases)
        {
            if (entry == null)
                continue;

            if (entry.category == category)
                return entry.database;
        }

        return null;
    }

    private void ClearShop()
    {
        if (contentParent == null)
            return;

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        instantiatedItems.Clear();
    }

    public void TryBuyProduct(ProductData product, ShopItemUI itemUI)
    {
        if (product == null)
        {
            Debug.LogError("TryBuyProduct: product está NULL.");
            return;
        }

        if (itemUI == null)
        {
            Debug.LogError("TryBuyProduct: itemUI está NULL.");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("TryBuyProduct: InventoryManager.Instance está NULL.");
            return;
        }

        if (FinanceManager.Instance == null)
        {
            Debug.LogError("TryBuyProduct: FinanceManager.Instance está NULL.");
            return;
        }

        if (InventoryManager.Instance.OwnsProduct(product.productId))
        {
            Debug.Log("Produto já comprado.");
            return;
        }

        if (!FinanceManager.Instance.HasEnoughMoney(product.preco))
        {
            Debug.Log("Dinheiro insuficiente.");
            return;
        }

        FinanceMovementData movement = FinanceManager.Instance.RegisterShopPurchaseExpense(
            $"Compra de {product.productName}",
            $"Produto comprado na loja. ID: {product.productId}",
            product.preco,
            FinanceMovementOrigin.ProductPurchase,
            true
        );

        if (movement == null)
        {
            Debug.Log("Não foi possível concluir a compra.");
            return;
        }

        InventoryManager.Instance.AddProduct(product);
        itemUI.RefreshState();

        Debug.Log($"Produto comprado: {product.productName}");
    }

    public void RefreshShopUI()
    {
        GenerateShopByCategory(currentCategory);
    }
}