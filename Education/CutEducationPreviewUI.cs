using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CutEducationPreviewUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text summaryText;

    public void SetData(AfroCutInfo cut)
    {
        if (cut == null)
        {
            Clear();
            return;
        }

        SetData(cut.cutName, cut.historicalSummary, cut.icon);
    }

    public void SetData(ClientRequestData request)
    {
        if (request == null)
        {
            Clear();
            return;
        }

        SetData(request.HistoryTitle, request.HistorySummary, request.icon);
    }

    public void SetData(string title, string summary, Sprite icon = null)
    {
        bool hasContent =
            !string.IsNullOrWhiteSpace(title) ||
            !string.IsNullOrWhiteSpace(summary) ||
            icon != null;

        if (root != null)
            root.SetActive(hasContent);

        gameObject.SetActive(hasContent);

        if (!hasContent)
            return;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        if (titleText != null)
            titleText.text = title;

        if (summaryText != null)
            summaryText.text = summary;
    }

    public void Clear()
    {
        if (root != null)
            root.SetActive(false);

        if (titleText != null)
            titleText.text = string.Empty;

        if (summaryText != null)
            summaryText.text = string.Empty;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }
}