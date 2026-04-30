using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RequestRequirementSlotUI : MonoBehaviour
{
    [SerializeField] private Image productIcon;
    [SerializeField] private GameObject checkObject;
    [SerializeField] private GameObject xObject;
    [SerializeField] private Button changeButton;
    [SerializeField] private TMP_Text tooltipText;
    [SerializeField] private Image backgroundImage;

    [Header("Cores")]
    [SerializeField] private Color defaultColor = new Color32(35, 45, 55, 255);

    public void Setup(
        ClientRequestData request,
        ServiceRequirementData req,
        ProductData selectedProduct,
        bool available,
        ClientRequestUI ui)
    {
        if (productIcon != null)
        {
            productIcon.sprite = selectedProduct != null ? selectedProduct.icon : null;
            productIcon.enabled = selectedProduct != null;
        }

        if (checkObject != null)
            checkObject.SetActive(false);

        if (xObject != null)
            xObject.SetActive(false);

        if (tooltipText != null)
            tooltipText.text = req != null ? req.GetDisplayName() : "";

        if (backgroundImage != null)
            backgroundImage.color = defaultColor;

        if (changeButton != null)
        {
            changeButton.onClick.RemoveAllListeners();
            changeButton.interactable = false;
            changeButton.gameObject.SetActive(false);
        }
    }
}