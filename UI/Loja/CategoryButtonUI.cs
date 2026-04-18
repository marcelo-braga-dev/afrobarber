using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryButtonUI : MonoBehaviour
{
    [Header("Configuração")]
    [SerializeField] private ProductCategory category;
    [SerializeField] private Button button;

    [Header("Visual")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text buttonText;

    [Header("Cores")]
    [SerializeField] private Color normalColor = new Color32(255, 166, 0, 255);
    [SerializeField] private Color selectedColor = new Color32(80, 45, 10, 255);
    [SerializeField] private Color normalTextColor = Color.black;
    [SerializeField] private Color selectedTextColor = Color.white;

    private ShopManager shopManager;

    public ProductCategory Category => category;

    public void Setup(ShopManager manager)
    {
        shopManager = manager;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickButton);
        }

        if (buttonText != null)
            buttonText.text = category.ToString();
    }

    private void OnClickButton()
    {
        if (shopManager != null)
            shopManager.SelectCategory(category);
    }

    public void SetSelected(bool selected)
    {
        if (backgroundImage != null)
            backgroundImage.color = selected ? selectedColor : normalColor;

        if (buttonText != null)
            buttonText.color = selected ? selectedTextColor : normalTextColor;
    }
}