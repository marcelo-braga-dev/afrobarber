using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudChatCardUI : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private TMP_Text messagePrefab;
    [SerializeField] private int maxVisibleMessages = 80;

    private void OnEnable()
    {
        if (GlobalDialogueManager.Instance != null)
            GlobalDialogueManager.Instance.OnMessageAdded += HandleMessage;
    }

    private void OnDisable()
    {
        if (GlobalDialogueManager.Instance != null)
            GlobalDialogueManager.Instance.OnMessageAdded -= HandleMessage;
    }

    private void Start()
    {
        if (GlobalDialogueManager.Instance == null)
            return;

        foreach (ChatMessageData item in GlobalDialogueManager.Instance.History)
            AddMessageUI(item);
    }

    private void HandleMessage(ChatMessageData message)
    {
        AddMessageUI(message);
    }

    private void AddMessageUI(ChatMessageData message)
    {
        if (contentRoot == null || messagePrefab == null || message == null)
            return;

        TMP_Text row = Instantiate(messagePrefab, contentRoot);
        row.text = message.GetHudFormattedText();

        while (contentRoot.childCount > maxVisibleMessages)
            Destroy(contentRoot.GetChild(0).gameObject);

        Canvas.ForceUpdateCanvases();

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }
}
