using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProductDatabase", menuName = "AfroBarber/Product Database")]
public class ProductDatabase : ScriptableObject
{
    public List<ProductData> products = new List<ProductData>();

    public ProductData GetProductById(string id)
    {
        return products.Find(p => p != null && p.productId == id);
    }
}