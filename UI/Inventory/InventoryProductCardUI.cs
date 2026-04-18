using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryProductCardUI : MonoBehaviour
{
    [Header("UI Principal")]
    [SerializeField] private Image productIcon;
    [SerializeField] private TMP_Text productNameText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text remainingText;
    [SerializeField] private TMP_Text estimatedLifeText;

    [Header("Atributos")]
    [SerializeField] private TMP_Text precisaoText;
    [SerializeField] private TMP_Text velocidadeText;
    [SerializeField] private TMP_Text durabilidadeText;

    [Header("Barra de condição")]
    [SerializeField] private Slider conditionSlider;

    [Header("Descartar")]
    [SerializeField] private Button discardButton;
    [SerializeField] private TMP_Text discardButtonText;

    private ProductData currentProduct;
    private ProductInventoryState currentState;

    public void Setup(ProductData product, ProductInventoryState state)
    {
        currentProduct = product;
        currentState = state;

        if (currentProduct == null || currentState == null)
            return;

        if (discardButton != null)
        {
            discardButton.onClick.RemoveAllListeners();
            discardButton.onClick.AddListener(OnClickDiscard);
        }

        RefreshState();
    }

    public void RefreshState()
    {
        if (currentProduct == null || currentState == null)
            return;

        if (productIcon != null)
            productIcon.sprite = currentProduct.icon;

        if (productNameText != null)
            productNameText.text = currentProduct.productName;

        if (statusText != null)
            statusText.text = currentState.GetStatusText(currentProduct);

        if (remainingText != null)
            remainingText.text = currentState.GetRemainingText(currentProduct);

        if (estimatedLifeText != null)
            estimatedLifeText.text = currentState.GetEstimatedLifeText(currentProduct);

        if (precisaoText != null)
            precisaoText.text = $"{currentProduct.precisao}";

        if (velocidadeText != null)
            velocidadeText.text = $"{currentProduct.velocidade}";

        if (durabilidadeText != null)
            durabilidadeText.text = $"{currentProduct.durabilidade}";

        if (conditionSlider != null)
            conditionSlider.value = currentState.GetNormalized();

        UpdateDiscardButton();
    }

    private void UpdateDiscardButton()
    {
        if (discardButton == null)
            return;

        bool canDiscard = !currentState.IsUsable(currentProduct);

        discardButton.gameObject.SetActive(canDiscard);

        if (discardButtonText != null)
        {
            discardButtonText.text = "Descartar";
        }
    }

    private void OnClickDiscard()
    {
        if (currentState == null)
            return;

        if (InventoryManager.Instance == null)
            return;

        bool removed = InventoryManager.Instance.RemoveItemByUniqueId(currentState.uniqueId);

        if (removed)
        {
            Debug.Log($"Item descartado: {currentProduct.productName}");

            if (InventoryUIManager.Instance != null)
            {
                InventoryUIManager.Instance.RefreshUI();
            }
        }
    }
}