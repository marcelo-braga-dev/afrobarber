using UnityEngine;

public class ProductDurabilitySystem : MonoBehaviour
{
    public static ProductDurabilitySystem Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            PersistentGameObject.MakePersistent(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool RegisterHoursUsage(string productId, int hoursUsed)
    {
        if (InventoryManager.Instance == null)
            return false;

        return InventoryManager.Instance.ConsumeDurableHours(productId, hoursUsed);
    }

    public bool RegisterServiceUsage(string productId, int uses = 1)
    {
        if (InventoryManager.Instance == null)
            return false;

        ProductData product = InventoryManager.Instance.GetProductDataById(productId);
        if (product == null)
            return false;

        int amountToConsume = Mathf.Max(1, product.consumoPorUso * uses);
        return InventoryManager.Instance.ConsumeProductUsage(productId, amountToConsume);
    }

    public bool HasUsableItem(string productId)
    {
        if (InventoryManager.Instance == null)
            return false;

        ProductInventoryState item = InventoryManager.Instance.GetFirstUsableItem(productId);
        return item != null;
    }
}