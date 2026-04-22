using TMPro;
using UnityEngine;

public class HudChatMessageItemUI : MonoBehaviour
{
    [Header("Textos")]
    [SerializeField] private TMP_Text authorText;
    [SerializeField] private TMP_Text messageText;

    public void Setup(string author, string message, Color authorColor)
    {
        if (authorText != null)
        {
            authorText.text = string.IsNullOrWhiteSpace(author) ? "" : $"{author}:";
            authorText.color = authorColor;
        }

        if (messageText != null)
        {
            messageText.text = message ?? "";
        }
    }
}