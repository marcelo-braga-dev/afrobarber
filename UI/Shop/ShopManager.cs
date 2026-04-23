using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Bancos por Categoria")]
    [SerializeField] private List<CategoryDatabaseEntry> categoryDatabases = new List<CategoryDatabaseEntry>();

    [Header("UI")]
    [SerializeField] private Transform productListContent;
    [SerializeField] private ShopItemUI productItemPrefab;

    [Header("Botões de categoria")]
    [SerializeField] private List<CategoryButtonUI> categoryButtons = new List<CategoryButtonUI>();

    private ProductCategory currentCategory;
    private bool hasSelectedCategory;

    private void Start()
    {
        SetupCategoryButtons();

        if (categoryDatabases != null && categoryDatabases.Count > 0)
        {
            SelectCategory(categoryDatabases[0].category);
        }
        else
        {
            RefreshShopUI();
        }
    }

    private void SetupCategoryButtons()
    {
        if (categoryButtons == null)
            return;

        foreach (CategoryButtonUI button in categoryButtons)
        {
            if (button == null)
                continue;

            button.Setup(this);
            button.SetSelected(hasSelectedCategory && button.Category == currentCategory);
        }
    }

    public void SelectCategory(ProductCategory category)
    {
        currentCategory = category;
        hasSelectedCategory = true;

        RefreshCategoryButtons();
        ShowCategory(category);
    }

    public void ShowCategory(ProductCategory category)
    {
        currentCategory = category;
        hasSelectedCategory = true;

        ClearProducts();

        ProductDatabase database = GetDatabaseByCategory(category);

        if (database == null || database.products == null)
        {
            Debug.LogWarning($"[ShopManager] Nenhum banco encontrado para a categoria: {category}");
            return;
        }

        foreach (ProductData product in database.products)
        {
            if (product == null)
                continue;

            if (!product.availableAtStart)
                continue;

            CreateProductCard(product);
        }
    }

    public void RefreshShopUI()
    {
        if (hasSelectedCategory)
        {
            ShowCategory(currentCategory);
            RefreshCategoryButtons();
            return;
        }

        if (categoryDatabases != null && categoryDatabases.Count > 0)
        {
            SelectCategory(categoryDatabases[0].category);
            return;
        }

        ClearProducts();
    }

    public void RefreshCurrentCategory()
    {
        RefreshShopUI();
    }

    private ProductDatabase GetDatabaseByCategory(ProductCategory category)
    {
        if (categoryDatabases == null)
            return null;

        foreach (CategoryDatabaseEntry entry in categoryDatabases)
        {
            if (entry == null)
                continue;

            if (entry.category == category)
                return entry.database;
        }

        return null;
    }

    private void CreateProductCard(ProductData product)
    {
        if (productListContent == null || productItemPrefab == null)
        {
            Debug.LogWarning("[ShopManager] Content ou prefab do produto não configurado.");
            return;
        }

        ShopItemUI itemUI = Instantiate(productItemPrefab, productListContent);
        itemUI.Setup(product, this);
    }

    private void ClearProducts()
    {
        if (productListContent == null)
            return;

        foreach (Transform child in productListContent)
        {
            Destroy(child.gameObject);
        }
    }

    private void RefreshCategoryButtons()
    {
        if (categoryButtons == null)
            return;

        foreach (CategoryButtonUI button in categoryButtons)
        {
            if (button == null)
                continue;

            button.SetSelected(hasSelectedCategory && button.Category == currentCategory);
        }
    }

    public bool TryBuyProduct(ProductData product, ShopItemUI itemUI)
    {
        if (product == null)
        {
            Debug.LogWarning("[ShopManager] Produto inválido.");
            return false;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[ShopManager] InventoryManager não encontrado.");
            return false;
        }

        if (InventoryManager.Instance.OwnsProduct(product.productId))
        {
            Debug.Log($"[ShopManager] Produto já comprado: {product.productName}");

            if (itemUI != null)
                itemUI.RefreshState();

            return false;
        }

        if (FinanceManager.Instance == null)
        {
            Debug.LogWarning("[ShopManager] FinanceManager não encontrado.");
            return false;
        }

        if (!FinanceManager.Instance.HasEnoughMoney(product.preco))
        {
            Debug.Log($"[ShopManager] Dinheiro insuficiente para comprar: {product.productName}");
            return false;
        }

        FinanceMovementData movement = FinanceManager.Instance.RegisterShopPurchaseExpense(
            $"Compra: {product.productName}",
            product.description,
            product.preco,
            FinanceMovementOrigin.ProductPurchase,
            true
        );

        if (movement == null)
        {
            Debug.LogWarning($"[ShopManager] Não foi possível registrar a compra: {product.productName}");
            return false;
        }

        InventoryManager.Instance.AddProduct(product);

        if (itemUI != null)
            itemUI.RefreshState();

        Debug.Log($"[ShopManager] Produto comprado: {product.productName} por R$ {product.preco}");

        return true;
    }
}