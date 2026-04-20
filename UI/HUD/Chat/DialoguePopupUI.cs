using System.Collections;
using TMPro;
using UnityEngine;

public class DialoguePopupUI : MonoBehaviour
{
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private float visibleSeconds = 2.5f;
    [SerializeField] private bool showOnlyNpcOrEventMessages = true;

    private Coroutine routine;

    private void OnEnable()
    {
        if (GlobalDialogueManager.Instance != null)
            GlobalDialogueManager.Instance.OnMessageAdded += OnMessageAdded;
    }

    private void OnDisable()
    {
        if (GlobalDialogueManager.Instance != null)
            GlobalDialogueManager.Instance.OnMessageAdded -= OnMessageAdded;
    }

    private void OnMessageAdded(ChatMessageData message)
    {
        if (message == null)
            return;

        if (showOnlyNpcOrEventMessages && message.messageType == ChatMessageType.Player)
            return;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(ShowRoutine(message.GetHudFormattedText()));
    }

    private IEnumerator ShowRoutine(string text)
    {
        if (popupRoot != null)
            popupRoot.SetActive(true);

        if (popupText != null)
            popupText.text = text;

        yield return new WaitForSeconds(visibleSeconds);

        if (popupRoot != null)
            popupRoot.SetActive(false);

        routine = null;
    }
}
