using UnityEngine;

public class InventoryToggleUI : MonoBehaviour
{
    [Header("Referência da UI")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("Configuração")]
    [SerializeField] private KeyCode toggleKey = KeyCode.I;
    [SerializeField] private bool startClosed = true;

    private bool isOpen = false;

    private void Start()
    {
        if (inventoryPanel == null)
        {
            Debug.LogWarning("InventoryToggleUI: inventoryPanel não atribuído.");
            return;
        }

        isOpen = !startClosed;
        inventoryPanel.SetActive(!startClosed);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (inventoryPanel == null)
            return;

        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);

        if (isOpen && InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.RefreshUI();
        }
    }

    public void OpenInventory()
    {
        if (inventoryPanel == null)
            return;

        isOpen = true;
        inventoryPanel.SetActive(true);

        if (InventoryUIManager.Instance != null)
        {
            InventoryUIManager.Instance.RefreshUI();
        }
    }

    public void CloseInventory()
    {
        if (inventoryPanel == null)
            return;

        isOpen = false;
        inventoryPanel.SetActive(false);
    }
}