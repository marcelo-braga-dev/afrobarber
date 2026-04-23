using System.Collections.Generic;
using UnityEngine;

public class BarbershopEnvironmentStats : MonoBehaviour
{
    public static BarbershopEnvironmentStats Instance { get; private set; }

    [Header("Produtos colocados no cenário")]
    [SerializeField] private List<ProductData> placedProducts = new List<ProductData>();

    [Header("Itens do inventário colocados no cenário")]
    [SerializeField] private List<ProductInventoryState> placedInventoryItems = new List<ProductInventoryState>();

    [Header("Valores padrão")]
    [Range(0f, 5f)]
    [SerializeField] private float defaultComfortScore = 3f;

    [Range(0f, 5f)]
    [SerializeField] private float defaultAestheticScore = 3f;

    public float ComfortScore => CalculateComfortScore();
    public float AestheticScore => CalculateAestheticScore();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterPlacedProduct(ProductData product)
    {
        if (product == null)
            return;

        if (!placedProducts.Contains(product))
            placedProducts.Add(product);
    }

    public void UnregisterPlacedProduct(ProductData product)
    {
        if (product == null)
            return;

        if (placedProducts.Contains(product))
            placedProducts.Remove(product);
    }

    public void RegisterPlacedInventoryItem(ProductInventoryState item)
    {
        if (item == null)
            return;

        if (!placedInventoryItems.Contains(item))
            placedInventoryItems.Add(item);
    }

    public void UnregisterPlacedInventoryItem(ProductInventoryState item)
    {
        if (item == null)
            return;

        if (placedInventoryItems.Contains(item))
            placedInventoryItems.Remove(item);
    }

    public float CalculateComfortScore()
    {
        float total = 0f;
        int count = 0;

        AddComfortFromProducts(placedProducts, ref total, ref count);
        AddComfortFromInventoryItems(placedInventoryItems, ref total, ref count);

        if (count <= 0)
            return defaultComfortScore;

        float average0To100 = total / count;
        return Convert0To100To0To5(average0To100);
    }

    public float CalculateAestheticScore()
    {
        float total = 0f;
        int count = 0;

        AddAestheticFromProducts(placedProducts, ref total, ref count);
        AddAestheticFromInventoryItems(placedInventoryItems, ref total, ref count);

        if (count <= 0)
            return defaultAestheticScore;

        float average0To100 = total / count;
        return Convert0To100To0To5(average0To100);
    }

    private void AddComfortFromProducts(List<ProductData> products, ref float total, ref int count)
    {
        if (products == null)
            return;

        foreach (ProductData product in products)
        {
            if (product == null)
                continue;

            if (product.conforto <= 0)
                continue;

            total += product.conforto;
            count++;
        }
    }

    private void AddAestheticFromProducts(List<ProductData> products, ref float total, ref int count)
    {
        if (products == null)
            return;

        foreach (ProductData product in products)
        {
            if (product == null)
                continue;

            if (product.estetica <= 0)
                continue;

            total += product.estetica;
            count++;
        }
    }

    private void AddComfortFromInventoryItems(List<ProductInventoryState> items, ref float total, ref int count)
    {
        if (items == null || InventoryManager.Instance == null)
            return;

        foreach (ProductInventoryState item in items)
        {
            if (item == null)
                continue;

            ProductData product = InventoryManager.Instance.GetProductDataById(item.productId);

            if (product == null || product.conforto <= 0)
                continue;

            total += product.conforto;
            count++;
        }
    }

    private void AddAestheticFromInventoryItems(List<ProductInventoryState> items, ref float total, ref int count)
    {
        if (items == null || InventoryManager.Instance == null)
            return;

        foreach (ProductInventoryState item in items)
        {
            if (item == null)
                continue;

            ProductData product = InventoryManager.Instance.GetProductDataById(item.productId);

            if (product == null || product.estetica <= 0)
                continue;

            total += product.estetica;
            count++;
        }
    }

    private float Convert0To100To0To5(float value)
    {
        return Mathf.Clamp(value / 100f * 5f, 0f, 5f);
    }
}