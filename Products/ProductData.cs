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

    [Header("Controle de atributos na loja")]
    public bool showAttributesInShop = true;
    public bool showPrecisao = true;
    public bool showVelocidade = true;
    public bool showDurabilidade = true;
    public bool showConforto = false;
    public bool showEstetica = false;
    public bool showTempoEntrega = true;

    [Header("Atributos do item")]
    [Range(0, 100)] public int precisao;
    [Range(0, 100)] public int velocidade;
    [Range(0, 100)] public int durabilidade;
    [Range(0, 100)] public int conforto;
    [Range(0, 100)] public int estetica;
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

    public bool HasAnyVisibleAttribute()
    {
        if (!showAttributesInShop)
            return false;

        return showPrecisao ||
               showVelocidade ||
               showDurabilidade ||
               showConforto ||
               showEstetica ||
               showTempoEntrega;
    }

    public float ConvertAttribute0To100ToScore0To5(int value)
    {
        return Mathf.Clamp(value / 100f * 5f, 0f, 5f);
    }

    public float GetPrecisaoScore0To5()
    {
        return ConvertAttribute0To100ToScore0To5(precisao);
    }

    public float GetVelocidadeScore0To5()
    {
        return ConvertAttribute0To100ToScore0To5(velocidade);
    }

    public float GetDurabilidadeScore0To5()
    {
        return ConvertAttribute0To100ToScore0To5(durabilidade);
    }

    public float GetConfortoScore0To5()
    {
        return ConvertAttribute0To100ToScore0To5(conforto);
    }

    public float GetEsteticaScore0To5()
    {
        return ConvertAttribute0To100ToScore0To5(estetica);
    }

    public float GetAverageToolQualityScore0To5()
    {
        float total = 0f;
        int count = 0;

        if (precisao > 0)
        {
            total += GetPrecisaoScore0To5();
            count++;
        }

        if (velocidade > 0)
        {
            total += GetVelocidadeScore0To5();
            count++;
        }

        if (durabilidade > 0)
        {
            total += GetDurabilidadeScore0To5();
            count++;
        }

        if (count <= 0)
            return 3f;

        return Mathf.Clamp(total / count, 0f, 5f);
    }
}