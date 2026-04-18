using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [Header("Referências UI")]
    [SerializeField] private Image productIcon;
    [SerializeField] private TMP_Text productNameText;
    [SerializeField] private TMP_Text productPriceText;
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text buttonText;

    [Header("Atributos")]
    [SerializeField] private TMP_Text precisaoText;
    [SerializeField] private Slider precisaoSlider;
    [SerializeField] private TMP_Text velocidadeText;
    [SerializeField] private Slider velocidadeSlider;
    [SerializeField] private TMP_Text durabilidadeText;
    [SerializeField] private Slider durabilidadeSlider;
    [SerializeField] private TMP_Text tempoEntregaText;

    private ProductData currentProduct;
    private ShopManager shopManager;

    public void Setup(ProductData product, ShopManager manager)
    {
        currentProduct = product;
        shopManager = manager;

        if (productIcon != null) productIcon.sprite = product.icon;
        if (productNameText != null) productNameText.text = product.productName;
        if (productPriceText != null) productPriceText.text = $"R${product.preco},00";
        if (precisaoText != null) precisaoText.text = $"{product.precisao}";
        if (velocidadeText != null) velocidadeText.text = $"{product.velocidade}";
        if (durabilidadeText != null) durabilidadeText.text = $"{product.durabilidade}";
        if (tempoEntregaText != null) tempoEntregaText.text = $"{product.tempoEntrega}h";

        if (precisaoSlider != null) precisaoSlider.value = product.precisao;
        if (velocidadeSlider != null) velocidadeSlider.value = product.velocidade;
        if (durabilidadeSlider != null) durabilidadeSlider.value = product.durabilidade;

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuyProduct);
        }

        RefreshState();
    }

    public void RefreshState()
    {
        if (currentProduct == null)
            return;

        if (InventoryManager.Instance != null && InventoryManager.Instance.OwnsProduct(currentProduct.productId))
        {
            if (buttonText != null) buttonText.text = "Comprado";
            if (buyButton != null) buyButton.interactable = false;
        }
        else
        {
            if (buttonText != null) buttonText.text = "Comprar";
            if (buyButton != null) buyButton.interactable = true;
        }
    }

    private void BuyProduct()
    {
        if (shopManager != null && currentProduct != null)
            shopManager.TryBuyProduct(currentProduct, this);
    }
}