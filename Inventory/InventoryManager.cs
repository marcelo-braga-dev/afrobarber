using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Bancos por Categoria")]
    [SerializeField] private List<CategoryDatabaseEntry> categoryDatabases = new List<CategoryDatabaseEntry>();

    private const string InventorySaveKey = "AFROBARBER_INVENTORY_ITEMS";

    [System.Serializable]
    private class InventorySaveData
    {
        public List<ProductInventoryState> items = new List<ProductInventoryState>();
    }

    private readonly List<ProductInventoryState> ownedItems = new List<ProductInventoryState>();
    private readonly Dictionary<string, ProductData> productLookup = new Dictionary<string, ProductData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildProductLookup();
            LoadInventory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void BuildProductLookup()
    {
        productLookup.Clear();

        foreach (CategoryDatabaseEntry entry in categoryDatabases)
        {
            if (entry == null || entry.database == null || entry.database.products == null)
                continue;

            foreach (ProductData product in entry.database.products)
            {
                if (product == null || string.IsNullOrWhiteSpace(product.productId))
                    continue;

                if (!productLookup.ContainsKey(product.productId))
                    productLookup.Add(product.productId, product);
            }
        }
    }

    public bool OwnsProduct(string productId)
    {
        return ownedItems.Exists(item => item.productId == productId);
    }

    public ProductInventoryState AddProduct(ProductData product)
    {
        if (product == null)
            return null;

        ProductInventoryState newItem = new ProductInventoryState(product);
        ownedItems.Add(newItem);
        SaveInventory();
        return newItem;
    }

    public ProductData GetProductDataById(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
            return null;

        productLookup.TryGetValue(productId, out ProductData product);
        return product;
    }

    public ProductInventoryState GetItemByUniqueId(string uniqueId)
    {
        if (string.IsNullOrWhiteSpace(uniqueId))
            return null;

        return ownedItems.Find(x => x.uniqueId == uniqueId);
    }

    public List<ProductInventoryState> GetAllOwnedItems()
    {
        return new List<ProductInventoryState>(ownedItems);
    }

    public List<ProductCategory> GetOwnedCategories()
    {
        HashSet<ProductCategory> categories = new HashSet<ProductCategory>();

        foreach (ProductInventoryState item in ownedItems)
        {
            ProductData product = GetProductDataById(item.productId);
            if (product != null)
                categories.Add(product.category);
        }

        return new List<ProductCategory>(categories);
    }

    public List<ProductInventoryState> GetItemsByCategory(ProductCategory category)
    {
        List<ProductInventoryState> result = new List<ProductInventoryState>();

        foreach (ProductInventoryState item in ownedItems)
        {
            ProductData product = GetProductDataById(item.productId);
            if (product != null && product.category == category)
                result.Add(item);
        }

        return result;
    }

    public List<ProductInventoryState> GetUsableItemsByCategory(ProductCategory category)
    {
        List<ProductInventoryState> result = new List<ProductInventoryState>();

        foreach (ProductInventoryState item in ownedItems)
        {
            ProductData product = GetProductDataById(item.productId);

            if (product == null)
                continue;

            if (product.category != category)
                continue;

            if (!item.IsUsable(product))
                continue;

            result.Add(item);
        }

        return result;
    }

    public ProductInventoryState GetFirstUsableItem(string productId)
    {
        foreach (ProductInventoryState item in ownedItems)
        {
            if (item.productId != productId)
                continue;

            ProductData product = GetProductDataById(item.productId);
            if (product != null && item.IsUsable(product))
                return item;
        }

        return null;
    }

    public List<ProductInventoryState> GetUsableItemsForRequirement(ServiceRequirementData requirement)
    {
        List<ProductInventoryState> result = new List<ProductInventoryState>();

        if (requirement == null)
            return result;

        foreach (ProductInventoryState item in ownedItems)
        {
            ProductData product = GetProductDataById(item.productId);

            if (product == null)
                continue;

            if (!item.IsUsable(product))
                continue;

            if (requirement.RequiresSpecificProduct)
            {
                if (product.productId == requirement.specificProductId)
                    result.Add(item);
            }
            else
            {
                if (product.category == requirement.category)
                    result.Add(item);
            }
        }

        return result;
    }

    public bool HasUsableItemForRequirement(ServiceRequirementData requirement)
    {
        return GetUsableItemsForRequirement(requirement).Count > 0;
    }

    public bool HasAllRequirements(List<ServiceRequirementData> requirements)
    {
        if (requirements == null || requirements.Count == 0)
            return true;

        foreach (ServiceRequirementData requirement in requirements)
        {
            if (requirement == null)
                continue;

            if (!HasUsableItemForRequirement(requirement))
                return false;
        }

        return true;
    }

    public bool ConsumeDurableHours(string productId, int hours)
    {
        ProductInventoryState item = GetFirstUsableItem(productId);
        ProductData product = GetProductDataById(productId);

        if (item == null || product == null)
            return false;

        if (product.inventoryItemType != InventoryItemType.Duravel)
            return false;

        item.Consume(product, Mathf.Max(1, hours));
        SaveInventory();
        return true;
    }

    public bool ConsumeProductUsage(string productId, int amount)
    {
        ProductInventoryState item = GetFirstUsableItem(productId);
        ProductData product = GetProductDataById(productId);

        if (item == null || product == null)
            return false;

        item.Consume(product, Mathf.Max(1, amount));
        SaveInventory();
        return true;
    }

    public bool ConsumeDurableHoursByUniqueId(string uniqueId, int hours)
    {
        ProductInventoryState item = GetItemByUniqueId(uniqueId);
        if (item == null)
            return false;

        ProductData product = GetProductDataById(item.productId);
        if (product == null)
            return false;

        if (product.inventoryItemType != InventoryItemType.Duravel)
            return false;

        item.Consume(product, Mathf.Max(1, hours));
        SaveInventory();
        return true;
    }

    public bool ConsumeProductUsageByUniqueId(string uniqueId, int amount)
    {
        ProductInventoryState item = GetItemByUniqueId(uniqueId);
        if (item == null)
            return false;

        ProductData product = GetProductDataById(item.productId);
        if (product == null)
            return false;

        item.Consume(product, Mathf.Max(1, amount));
        SaveInventory();
        return true;
    }

    public bool RemoveItemByUniqueId(string uniqueId)
    {
        ProductInventoryState item = GetItemByUniqueId(uniqueId);
        if (item == null)
            return false;

        bool removed = ownedItems.Remove(item);

        if (removed)
            SaveInventory();

        return removed;
    }

    public int RemoveAllUnusableItems()
    {
        int removedCount = 0;

        for (int i = ownedItems.Count - 1; i >= 0; i--)
        {
            ProductInventoryState item = ownedItems[i];
            ProductData product = GetProductDataById(item.productId);

            if (product == null)
            {
                ownedItems.RemoveAt(i);
                removedCount++;
                continue;
            }

            if (!item.IsUsable(product))
            {
                ownedItems.RemoveAt(i);
                removedCount++;
            }
        }

        if (removedCount > 0)
            SaveInventory();

        return removedCount;
    }

    public void SaveInventory()
    {
        InventorySaveData saveData = new InventorySaveData
        {
            items = ownedItems
        };

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(InventorySaveKey, json);
        PlayerPrefs.Save();
    }

    private void LoadInventory()
    {
        ownedItems.Clear();

        string json = PlayerPrefs.GetString(InventorySaveKey, "");

        if (string.IsNullOrWhiteSpace(json))
            return;

        InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);

        if (saveData == null || saveData.items == null)
            return;

        ownedItems.AddRange(saveData.items);
    }
}