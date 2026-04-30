using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientRequestUI : MonoBehaviour
{
    public static ClientRequestUI Instance { get; private set; }

    public bool IsOpen => rootPanel != null && rootPanel.activeSelf;

    [Header("Janela principal")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private Image requestIcon;
    [SerializeField] private TMP_Text requestNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text xpText;
    [SerializeField] private TMP_Text difficultyText;
    [SerializeField] private TMP_Text summaryText;
    [SerializeField] private Image readinessFillImage;

    [Header("Educação / História")]
    [SerializeField] private GameObject educationPanel;
    [SerializeField] private TMP_Text educationTitleText;
    [SerializeField] private TMP_Text educationSummaryText;

    [Header("Botões")]
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button dispenseButton;
    [SerializeField] private Button closeButton;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private ClientNPC currentClient;
    private ClientRequestData currentRequest;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            HideImmediate();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Show(ClientNPC client)
    {
        if (client == null)
            return;

        ClientRequestData request = GetClientRequest(client);

        if (request == null)
        {
            Debug.LogWarning("[ClientRequestUI] O cliente não possui RequestData.");
            return;
        }

        request.SyncCompatibilityFields();

        currentClient = client;
        currentRequest = request;

        if (rootPanel != null)
            rootPanel.SetActive(true);

        BindButtons();
        Refresh();

        if (enableDebugLogs)
            Debug.Log($"[ClientRequestUI] Show() aberto para {client.name} | Pedido: {currentRequest.RequestName}");
    }

    private ClientRequestData GetClientRequest(ClientNPC client)
    {
        if (client == null)
            return null;

        if (client.RequestData != null)
            return client.RequestData;

        if (client.CurrentRequest != null)
            return client.CurrentRequest;

        return null;
    }

    private void BindButtons()
    {
        if (acceptButton != null)
        {
            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(OnClickAccept);
        }

        if (dispenseButton != null)
        {
            dispenseButton.onClick.RemoveAllListeners();
            dispenseButton.onClick.AddListener(OnClickDispense);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }
    }

    private void Refresh()
    {
        if (currentRequest == null)
            return;

        currentRequest.SyncCompatibilityFields();

        if (requestIcon != null)
        {
            requestIcon.sprite = currentRequest.icon;
            requestIcon.enabled = currentRequest.icon != null;
        }

        if (requestNameText != null)
            requestNameText.text = currentRequest.RequestName;

        if (descriptionText != null)
            descriptionText.text = currentRequest.GetDescription();

        if (priceText != null)
            priceText.text = $"R$ {currentRequest.ServicePrice}";

        if (timeText != null)
            timeText.text = $"{currentRequest.ServiceTimeRoundedMinutes} min";

        if (xpText != null)
            xpText.text = $"+{currentRequest.XPReward} XP";

        if (difficultyText != null)
            difficultyText.text = $"Dificuldade {currentRequest.difficulty}";

        RefreshEducationalPanel();
        UpdateSummaryAndReadiness();

        if (acceptButton != null)
            acceptButton.gameObject.SetActive(true);

        if (dispenseButton != null)
            dispenseButton.gameObject.SetActive(true);
    }

    private void RefreshEducationalPanel()
    {
        if (currentRequest == null)
        {
            if (educationPanel != null)
                educationPanel.SetActive(false);

            return;
        }

        AfroCutInfo cutInfo = null;

        if (currentClient != null)
            cutInfo = currentClient.GetCurrentCutInfo();

        string title = currentRequest.HistoryTitle;
        string summary = currentRequest.HistorySummary;

        if (cutInfo != null)
        {
            if (string.IsNullOrWhiteSpace(title))
                title = cutInfo.cutName;

            if (string.IsNullOrWhiteSpace(summary))
                summary = cutInfo.historicalSummary;
        }

        bool hasEducation =
            !string.IsNullOrWhiteSpace(title) ||
            !string.IsNullOrWhiteSpace(summary) ||
            cutInfo != null;

        if (educationPanel != null)
            educationPanel.SetActive(hasEducation);

        if (!hasEducation)
            return;

        if (educationTitleText != null)
            educationTitleText.text = title;

        if (educationSummaryText != null)
            educationSummaryText.text = summary;

        if (enableDebugLogs)
            Debug.Log($"[ClientRequestUI] Educação carregada | Title: {title} | Summary: {summary}");
    }

    private void UpdateSummaryAndReadiness()
    {
        if (summaryText != null)
            summaryText.text = "Aceite o cliente para iniciar o atendimento ou dispense o cliente.";

        if (readinessFillImage != null)
            readinessFillImage.fillAmount = 1f;
    }

    private void OnClickAccept()
    {
        if (currentClient == null || currentRequest == null)
            return;

        if (enableDebugLogs)
            Debug.Log($"[ClientRequestUI] Cliente aceito: {currentClient.name} | Pedido: {currentRequest.RequestName}");

        currentClient.CallForService();
        Hide();
    }

    private void OnClickDispense()
    {
        if (currentClient != null)
        {
            if (enableDebugLogs)
                Debug.Log($"[ClientRequestUI] Cliente dispensado: {currentClient.name}");

            currentClient.DispenseDueToMissingItems();
        }

        Hide();
    }

    public void Hide()
    {
        currentClient = null;
        currentRequest = null;

        HideImmediate();
    }

    private void HideImmediate()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }
}