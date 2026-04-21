using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServiceToolOptionItemUI : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Button button;
    [SerializeField] private Image productImage;
    [SerializeField] private GameObject selectedIndicator;

    [Header("Textos")]
    [SerializeField] private TMP_Text productNameText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text durabilityText;

    private ProductInventoryState state;
    private ProductData product;
    private Action<ProductInventoryState> onSelected;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(Select);
    }

    public void Setup(
        ProductInventoryState state,
        ProductData product,
        bool isSelected,
        Action<ProductInventoryState> selectCallback)
    {
        this.state = state;
        this.product = product;
        onSelected = selectCallback;

        Refresh(isSelected);
    }

    private void Refresh(bool isSelected)
    {
        if (selectedIndicator != null)
            selectedIndicator.SetActive(isSelected);

        if (productImage != null)
        {
            if (product != null && product.icon != null)
            {
                productImage.sprite = product.icon;
                productImage.enabled = true;
            }
            else
            {
                productImage.sprite = null;
                productImage.enabled = false;
            }
        }

        if (productNameText != null)
            productNameText.text = product != null ? product.productName : "Produto";

        if (statsText != null && product != null)
        {
            statsText.text =
                $"Precisão: {product.precisao:0}\n" +
                $"Velocidade: {product.velocidade:0}\n" +
                $"Durabilidade: {product.durabilidade:0}";
        }

        if (durabilityText != null)
        {
            if (state != null)
                durabilityText.text = $"Estado: {state.GetNormalized() * 100f:0}%";
            else
                durabilityText.text = "";
        }
    }

    private void Select()
    {
        if (state == null)
            return;

        onSelected?.Invoke(state);
    }
}