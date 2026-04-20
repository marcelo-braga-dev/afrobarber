using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudChatCardUI : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private TMP_Text messagePrefab;

    [Header("Configuração")]
    [SerializeField] private int maxVisibleMessages = 80;

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

        TMP_Text row = Instantiate(messagePrefab, contentRoot);
        row.gameObject.SetActive(true);
        row.text = message.GetHudFormattedText();

        while (contentRoot.childCount > maxVisibleMessages)
        {
            Destroy(contentRoot.GetChild(0).gameObject);
        }

        if (scrollToBottom)
        {
            ScrollToBottom();
        }
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
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }
    }
}