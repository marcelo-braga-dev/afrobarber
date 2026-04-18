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
    [SerializeField] private Color availableColor = new Color32(35, 45, 55, 255);
    [SerializeField] private Color missingColor = new Color32(70, 25, 25, 255);

    private ServiceRequirementData requirement;
    private ClientRequestData requestData;
    private ClientRequestUI parentUI;

    public void Setup(
        ClientRequestData request,
        ServiceRequirementData req,
        ProductData selectedProduct,
        bool available,
        ClientRequestUI ui)
    {
        requestData = request;
        requirement = req;
        parentUI = ui;

        if (productIcon != null)
            productIcon.sprite = selectedProduct != null ? selectedProduct.icon : null;

        if (checkObject != null)
            checkObject.SetActive(available);

        if (xObject != null)
            xObject.SetActive(!available);

        if (tooltipText != null)
            tooltipText.text = req.GetDisplayName();

        if (backgroundImage != null)
            backgroundImage.color = available ? availableColor : missingColor;

        if (changeButton != null)
        {
            changeButton.onClick.RemoveAllListeners();
            changeButton.onClick.AddListener(OnClickChange);
            changeButton.interactable = available;
        }
    }

    private void OnClickChange()
    {
        if (parentUI != null && requestData != null && requirement != null)
            parentUI.OpenSelectionForRequirement(requestData, requirement);
    }
}