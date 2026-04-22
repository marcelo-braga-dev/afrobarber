using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudChatCardUI : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private HudChatMessageItemUI messagePrefab;

    [Header("Configuração")]
    [SerializeField] private int maxVisibleMessages = 80;

    [Header("Cores fixas")]
    [SerializeField] private Color playerNameColor = new Color(0.25f, 0.75f, 1f);
    [SerializeField] private Color systemNameColor = new Color(1f, 0.8f, 0.25f);
    [SerializeField] private Color defaultNameColor = Color.white;

    [Header("Paleta de NPCs/Clientes")]
    [SerializeField]
    private Color[] npcNameColors =
    {
        new Color(0.95f, 0.45f, 0.25f),
        new Color(0.45f, 0.85f, 0.45f),
        new Color(0.75f, 0.55f, 1f),
        new Color(1f, 0.55f, 0.8f),
        new Color(0.4f, 0.9f, 0.9f),
        new Color(1f, 0.7f, 0.35f),
        new Color(0.65f, 0.85f, 1f),
        new Color(0.85f, 1f, 0.45f)
    };

    private readonly Dictionary<string, Color> fixedColorsBySpeaker = new Dictionary<string, Color>();

    private bool isSubscribed;

    private void OnEnable()
    {
        TrySubscribe();
        RebuildHistory();
    }

    private void Start()
    {
        TrySubscribe();
        RebuildHistory();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void TrySubscribe()
    {
        if (isSubscribed)
            return;

        if (GlobalDialogueManager.Instance == null)
        {
            Debug.LogWarning("[HudChatCardUI] GlobalDialogueManager.Instance ainda não existe. Tentando novamente em breve.");
            Invoke(nameof(TrySubscribe), 0.2f);
            return;
        }

        GlobalDialogueManager.Instance.OnMessageAdded -= HandleMessage;
        GlobalDialogueManager.Instance.OnMessageAdded += HandleMessage;

        isSubscribed = true;

        Debug.Log("[HudChatCardUI] Inscrito no OnMessageAdded com sucesso.");
    }

    private void Unsubscribe()
    {
        CancelInvoke(nameof(TrySubscribe));

        if (GlobalDialogueManager.Instance != null)
        {
            GlobalDialogueManager.Instance.OnMessageAdded -= HandleMessage;
        }

        isSubscribed = false;
    }

    private void RebuildHistory()
    {
        if (GlobalDialogueManager.Instance == null)
            return;

        if (contentRoot == null || messagePrefab == null)
            return;

        ClearContent();

        foreach (ChatMessageData item in GlobalDialogueManager.Instance.History)
        {
            AddMessageUI(item, false);
        }

        ScrollToBottom();
    }

    private void HandleMessage(ChatMessageData message)
    {
        AddMessageUI(message, true);
    }

    private void AddMessageUI(ChatMessageData message, bool scrollToBottom)
    {
        if (contentRoot == null)
        {
            Debug.LogWarning("[HudChatCardUI] Content Root não foi configurado.");
            return;
        }

        if (messagePrefab == null)
        {
            Debug.LogWarning("[HudChatCardUI] Message Prefab não foi configurado.");
            return;
        }

        if (message == null)
        {
            Debug.LogWarning("[HudChatCardUI] Mensagem recebida está nula.");
            return;
        }

        HudChatMessageItemUI row = Instantiate(messagePrefab, contentRoot);
        row.gameObject.SetActive(true);

        string author = ExtractAuthor(message);
        string text = ExtractMessageText(message);
        Color authorColor = GetColorForSpeaker(message, author);

        row.Setup(author, text, authorColor);

        while (contentRoot.childCount > maxVisibleMessages)
        {
            Destroy(contentRoot.GetChild(0).gameObject);
        }

        if (scrollToBottom)
        {
            ScrollToBottom();
        }
    }

    private string ExtractAuthor(ChatMessageData message)
    {
        if (message == null)
            return "";

        string formatted = message.GetHudFormattedText();

        if (string.IsNullOrWhiteSpace(formatted))
            return "";

        int separatorIndex = formatted.IndexOf(':');

        if (separatorIndex <= 0)
            return "";

        return formatted.Substring(0, separatorIndex).Trim();
    }

    private string ExtractMessageText(ChatMessageData message)
    {
        if (message == null)
            return "";

        string formatted = message.GetHudFormattedText();

        if (string.IsNullOrWhiteSpace(formatted))
            return "";

        int separatorIndex = formatted.IndexOf(':');

        if (separatorIndex < 0 || separatorIndex >= formatted.Length - 1)
            return formatted;

        return formatted.Substring(separatorIndex + 1).Trim();
    }

    private Color GetColorForSpeaker(ChatMessageData message, string author)
    {
        string speakerKey = GetSpeakerKey(message, author);

        if (string.IsNullOrWhiteSpace(speakerKey))
            return defaultNameColor;

        if (IsPlayerSpeaker(author, speakerKey))
            return playerNameColor;

        if (IsSystemSpeaker(author, speakerKey))
            return systemNameColor;

        if (fixedColorsBySpeaker.TryGetValue(speakerKey, out Color cachedColor))
            return cachedColor;

        Color generatedColor = GenerateColorForSpeaker(speakerKey);
        fixedColorsBySpeaker[speakerKey] = generatedColor;

        return generatedColor;
    }

    private string GetSpeakerKey(ChatMessageData message, string author)
    {
        if (!string.IsNullOrWhiteSpace(author))
            return author.Trim().ToLowerInvariant();

        string formatted = message != null ? message.GetHudFormattedText() : "";

        if (!string.IsNullOrWhiteSpace(formatted))
            return formatted.Trim().ToLowerInvariant();

        return "";
    }

    private bool IsPlayerSpeaker(string author, string speakerKey)
    {
        if (string.IsNullOrWhiteSpace(author) && string.IsNullOrWhiteSpace(speakerKey))
            return false;

        string value = !string.IsNullOrWhiteSpace(author) ? author : speakerKey;
        value = value.Trim().ToLowerInvariant();

        return value == "você" ||
               value == "voce" ||
               value == "player" ||
               value == "jogador";
    }

    private bool IsSystemSpeaker(string author, string speakerKey)
    {
        if (string.IsNullOrWhiteSpace(author) && string.IsNullOrWhiteSpace(speakerKey))
            return false;

        string value = !string.IsNullOrWhiteSpace(author) ? author : speakerKey;
        value = value.Trim().ToLowerInvariant();

        return value == "sistema" ||
               value == "system";
    }

    private Color GenerateColorForSpeaker(string speakerKey)
    {
        if (npcNameColors == null || npcNameColors.Length == 0)
            return defaultNameColor;

        int hash = Mathf.Abs(speakerKey.GetHashCode());
        int index = hash % npcNameColors.Length;

        return npcNameColors[index];
    }

    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void ClearContent()
    {
        if (contentRoot == null)
            return;

        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }
    }
}