using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; }

    [Header("Referências")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private ScrollRect categoriesScrollRect;
    [SerializeField] private InventoryCategoryGroupUI categoryGroupPrefab;
    [SerializeField] private InventoryProductCardUI productCardPrefab;

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        ClearUI();

        if (InventoryManager.Instance == null)
            return;

        List<ProductCategory> ownedCategories = InventoryManager.Instance.GetOwnedCategories();

        foreach (ProductCategory category in ownedCategories)
        {
            List<ProductInventoryState> items = InventoryManager.Instance.GetItemsByCategory(category);
            if (items == null || items.Count == 0)
                continue;

            InventoryCategoryGroupUI groupUI = Instantiate(categoryGroupPrefab, contentParent);
            groupUI.Setup(category, categoriesScrollRect);
            spawnedObjects.Add(groupUI.gameObject);

            foreach (ProductInventoryState item in items)
            {
                ProductData product = InventoryManager.Instance.GetProductDataById(item.productId);
                if (product == null)
                    continue;

                InventoryProductCardUI card = Instantiate(productCardPrefab, groupUI.ProductsParent);
                card.Setup(product, item);
                spawnedObjects.Add(card.gameObject);
            }
        }
    }

    private void ClearUI()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        spawnedObjects.Clear();

        if (contentParent != null)
        {
            for (int i = contentParent.childCount - 1; i >= 0; i--)
            {
                Destroy(contentParent.GetChild(i).gameObject);
            }
        }
    }
}