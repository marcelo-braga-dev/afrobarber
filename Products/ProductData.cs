using UnityEngine;

[CreateAssetMenu(fileName = "NewProduct", menuName = "AfroBarber/Product")]
public class ProductData : ScriptableObject
{
    [Header("Identificação")]
    public string productId;
    public string productName;
    [TextArea] public string description;

    [Header("Classificação")]
    public ProductCategory category;
    public ProductRarity rarity;
    public InventoryItemType inventoryItemType = InventoryItemType.Duravel;
    public InventoryUsageType usageType = InventoryUsageType.PorHoraDeUso;

    [Header("Visual")]
    public Sprite icon;
    public GameObject prefab;

    [Header("Economia")]
    public int preco;
    public bool availableAtStart = true;

    [Header("Atributos do item")]
    [Range(0, 100)] public int precisao;
    [Range(0, 100)] public int velocidade;
    [Range(0, 100)] public int durabilidade;
    public int tempoEntrega;

    [Header("Uso no cenário")]
    public bool canBePlacedInScene;
    public bool canBeUsedForHaircut = false;

    [Header("Inventário - Duráveis")]
    public int vidaUtilMinimaHoras = 100;

    [Header("Inventário - Consumíveis")]
    public int capacidadeConsumivel = 100;
    public int consumoPorUso = 1;

    [Header("Inventário - Caixa com unidades")]
    public int unidadesNaCaixa = 10;

    public int GetMaxVidaUtilHoras()
    {
        return Mathf.Max(1, vidaUtilMinimaHoras + durabilidade);
    }

    public int GetCapacidadeInicial()
    {
        switch (inventoryItemType)
        {
            case InventoryItemType.Duravel:
                return GetMaxVidaUtilHoras();

            case InventoryItemType.Consumivel:
                return Mathf.Max(1, capacidadeConsumivel);

            case InventoryItemType.CaixaComUnidades:
                return Mathf.Max(1, unidadesNaCaixa);

            default:
                return 1;
        }
    }
}