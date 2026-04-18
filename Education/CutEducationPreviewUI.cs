using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CutEducationPreviewUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text cutNameText;
    [SerializeField] private TMP_Text decadeText;
    [SerializeField] private TMP_Text culturalMeaningText;
    [SerializeField] private TMP_Text historicalSummaryText;

    public void SetData(AfroCutInfo cut)
    {
        if (cut == null)
        {
            if (root != null)
                root.SetActive(false);
            return;
        }

        if (root != null)
            root.SetActive(true);

        if (iconImage != null)
            iconImage.sprite = cut.icon;

        if (cutNameText != null)
            cutNameText.text = cut.cutName;

        if (decadeText != null)
            decadeText.text = "Período: " + cut.decade;

        if (culturalMeaningText != null)
            culturalMeaningText.text = cut.culturalMeaning;

        if (historicalSummaryText != null)
            historicalSummaryText.text = cut.historicalSummary;
    }

    public void Clear()
    {
        if (root != null)
            root.SetActive(false);
    }
}