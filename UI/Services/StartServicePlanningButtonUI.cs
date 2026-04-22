using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartServicePlanningButtonUI : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private BarbershopServiceManager serviceManager;
    [SerializeField] private Button startButton;
    [SerializeField] private GameObject root;

    [Header("Texto")]
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private string visibleButtonText = "Iniciar Atendimento";

    [Header("Configuração")]
    [SerializeField] private bool hideWhenCannotOpen = true;
    [SerializeField] private float refreshInterval = 0.2f;

    private float nextRefreshTime;

    private void Awake()
    {
        if (serviceManager == null)
            serviceManager = FindFirstObjectByType<BarbershopServiceManager>();

        if (startButton == null)
            startButton = GetComponentInChildren<Button>(true);

        if (root == null)
            root = gameObject;

        if (startButton != null)
            startButton.onClick.AddListener(HandleClick);

        if (buttonText != null)
            buttonText.text = visibleButtonText;

        RefreshVisibility();
    }

    private void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(HandleClick);
    }

    private void Update()
    {
        if (Time.time < nextRefreshTime)
            return;

        nextRefreshTime = Time.time + refreshInterval;
        RefreshVisibility();
    }

    private void HandleClick()
    {
        if (serviceManager == null)
            serviceManager = FindFirstObjectByType<BarbershopServiceManager>();

        if (serviceManager == null)
        {
            Debug.LogWarning("[StartServicePlanningButtonUI] BarbershopServiceManager não encontrado.");
            return;
        }

        serviceManager.TryOpenPlanningForCurrentClient();
        RefreshVisibility();
    }

    private void RefreshVisibility()
    {
        if (serviceManager == null)
            serviceManager = FindFirstObjectByType<BarbershopServiceManager>();

        bool canOpen = serviceManager != null && serviceManager.CanOpenPlanningForCurrentClient();

        if (root != null && hideWhenCannotOpen)
            root.SetActive(canOpen);

        if (startButton != null)
            startButton.interactable = canOpen;

        if (buttonText != null)
            buttonText.text = visibleButtonText;
    }
}