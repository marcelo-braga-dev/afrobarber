using UnityEngine;

public class DebugAddProductToInventory : MonoBehaviour
{
    [SerializeField] private ProductData productToAdd;
    [SerializeField] private KeyCode addKey = KeyCode.I;

    private void Update()
    {
        if (!Input.GetKeyDown(addKey))
            return;

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[DebugAddProductToInventory] InventoryManager.Instance não encontrado.");
            return;
        }

        if (productToAdd == null)
        {
            Debug.LogWarning("[DebugAddProductToInventory] productToAdd não configurado.");
            return;
        }

        ProductInventoryState addedItem = InventoryManager.Instance.AddProduct(productToAdd);

        if (addedItem == null)
        {
            Debug.LogWarning("[DebugAddProductToInventory] Falha ao adicionar produto.");
            return;
        }

        Debug.Log(
            $"[DebugAddProductToInventory] Produto adicionado: {productToAdd.productName} | " +
            $"ProductId: {productToAdd.productId} | UniqueId: {addedItem.uniqueId}"
        );
    }
}