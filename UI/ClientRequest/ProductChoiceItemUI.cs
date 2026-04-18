using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductChoiceItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private Slider conditionSlider;
    [SerializeField] private Button selectButton;

    public void Setup(ProductData product, ProductInventoryState state, System.Action onSelect)
    {
        if (iconImage != null)
            iconImage.sprite = product.icon;

        if (nameText != null)
            nameText.text = product.productName;

        if (infoText != null)
            infoText.text = $"{state.GetRemainingText(product)}\n{state.GetStatusText(product)}";

        if (conditionSlider != null)
            conditionSlider.value = state.GetNormalized();

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onSelect?.Invoke());
        }
    }
}