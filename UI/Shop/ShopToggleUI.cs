using UnityEngine;

public class ShopToggleUI : MonoBehaviour
{
    public static ShopToggleUI Instance { get; private set; }

    [Header("UI da Loja")]
    [SerializeField] private GameObject shopPanel;

    [Header("Configuração")]
    [SerializeField] private KeyCode toggleKey = KeyCode.O;
    [SerializeField] private bool startClosed = true;

    [Header("Opcional")]
    [SerializeField] private bool pauseGame = false;

    private bool isOpen = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (shopPanel == null)
        {
            Debug.LogWarning("ShopToggleUI: shopPanel não atribuído.");
            return;
        }

        isOpen = !startClosed;
        shopPanel.SetActive(!startClosed);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleShop();
        }
    }

    public void ToggleShop()
    {
        if (shopPanel == null)
            return;

        isOpen = !isOpen;
        shopPanel.SetActive(isOpen);

        if (pauseGame)
            Time.timeScale = isOpen ? 0f : 1f;

        if (isOpen)
        {
            RefreshShop();
        }
    }

    public void OpenShop()
    {
        if (shopPanel == null)
            return;

        isOpen = true;
        shopPanel.SetActive(true);

        if (pauseGame)
            Time.timeScale = 0f;

        RefreshShop();
    }

    public void CloseShop()
    {
        if (shopPanel == null)
            return;

        isOpen = false;
        shopPanel.SetActive(false);

        if (pauseGame)
            Time.timeScale = 1f;
    }

    private void RefreshShop()
    {
        ShopManager shop = FindFirstObjectByType<ShopManager>();

        if (shop != null)
        {
            shop.RefreshShopUI();
        }
    }
}