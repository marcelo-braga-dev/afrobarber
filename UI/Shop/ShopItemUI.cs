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

    [Header("Container geral dos atributos")]
    [SerializeField] private GameObject attributesContainer;

    [Header("Precisão")]
    [SerializeField] private GameObject precisaoRoot;
    [SerializeField] private TMP_Text precisaoText;
    [SerializeField] private Slider precisaoSlider;

    [Header("Velocidade")]
    [SerializeField] private GameObject velocidadeRoot;
    [SerializeField] private TMP_Text velocidadeText;
    [SerializeField] private Slider velocidadeSlider;

    [Header("Durabilidade")]
    [SerializeField] private GameObject durabilidadeRoot;
    [SerializeField] private TMP_Text durabilidadeText;
    [SerializeField] private Slider durabilidadeSlider;

    [Header("Conforto")]
    [SerializeField] private GameObject confortoRoot;
    [SerializeField] private TMP_Text confortoText;
    [SerializeField] private Slider confortoSlider;

    [Header("Estética")]
    [SerializeField] private GameObject esteticaRoot;
    [SerializeField] private TMP_Text esteticaText;
    [SerializeField] private Slider esteticaSlider;

    [Header("Tempo de entrega")]
    [SerializeField] private GameObject tempoEntregaRoot;
    [SerializeField] private TMP_Text tempoEntregaText;

    private ProductData currentProduct;
    private ShopManager shopManager;

    public void Setup(ProductData product, ShopManager manager)
    {
        currentProduct = product;
        shopManager = manager;

        if (product == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (productIcon != null)
        {
            productIcon.sprite = product.icon;
            productIcon.enabled = product.icon != null;
        }

        if (productNameText != null)
            productNameText.text = product.productName;

        if (productPriceText != null)
            productPriceText.text = $"R$ {product.preco:N0},00";

        SetupAttributes(product);

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuyProduct);
        }

        RefreshState();
    }

    private void SetupAttributes(ProductData product)
    {
        bool showAttributes = product != null && product.HasAnyVisibleAttribute();

        if (attributesContainer != null)
            attributesContainer.SetActive(showAttributes);

        if (product == null)
            return;

        SetupAttributeRow(
            precisaoRoot,
            precisaoText,
            precisaoSlider,
            showAttributes && product.showPrecisao,
            product.precisao
        );

        SetupAttributeRow(
            velocidadeRoot,
            velocidadeText,
            velocidadeSlider,
            showAttributes && product.showVelocidade,
            product.velocidade
        );

        SetupAttributeRow(
            durabilidadeRoot,
            durabilidadeText,
            durabilidadeSlider,
            showAttributes && product.showDurabilidade,
            product.durabilidade
        );

        SetupAttributeRow(
            confortoRoot,
            confortoText,
            confortoSlider,
            showAttributes && product.showConforto,
            product.conforto
        );

        SetupAttributeRow(
            esteticaRoot,
            esteticaText,
            esteticaSlider,
            showAttributes && product.showEstetica,
            product.estetica
        );

        SetupTempoEntrega(product, showAttributes);
    }

    private void SetupAttributeRow(
        GameObject root,
        TMP_Text valueText,
        Slider slider,
        bool show,
        int value)
    {
        if (root != null)
            root.SetActive(show);

        if (!show)
            return;

        int clampedValue = Mathf.Clamp(value, 0, 100);

        if (valueText != null)
            valueText.text = clampedValue.ToString();

        if (slider != null)
        {
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.wholeNumbers = true;
            slider.interactable = false;
            slider.value = clampedValue;
        }
    }

    private void SetupTempoEntrega(ProductData product, bool showAttributes)
    {
        bool show = showAttributes && product.showTempoEntrega;

        if (tempoEntregaRoot != null)
            tempoEntregaRoot.SetActive(show);

        if (!show)
            return;

        if (tempoEntregaText != null)
        {
            if (product.tempoEntrega <= 0)
                tempoEntregaText.text = "Imediato";
            else
                tempoEntregaText.text = $"{product.tempoEntrega}h";
        }
    }

    public void RefreshState()
    {
        if (currentProduct == null)
            return;

        bool alreadyBought =
            InventoryManager.Instance != null &&
            InventoryManager.Instance.OwnsProduct(currentProduct.productId);

        if (alreadyBought)
        {
            if (buttonText != null)
                buttonText.text = "Comprado";

            if (buyButton != null)
                buyButton.interactable = false;
        }
        else
        {
            if (buttonText != null)
                buttonText.text = "Comprar";

            if (buyButton != null)
                buyButton.interactable = true;
        }
    }

    private void BuyProduct()
    {
        if (shopManager == null || currentProduct == null)
            return;

        shopManager.TryBuyProduct(currentProduct, this);
    }
}