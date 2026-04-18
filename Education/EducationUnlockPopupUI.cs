using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EducationUnlockPopupUI : MonoBehaviour
{
    public static EducationUnlockPopupUI Instance;

    [SerializeField] private GameObject root;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private TMP_Text summaryText;

    private void Awake()
    {
        Instance = this;

        if (root != null)
            root.SetActive(false);
    }

    public void ShowUnlock(AfroCutInfo cut)
    {
        if (cut == null || root == null)
            return;

        root.SetActive(true);

        if (iconImage != null)
            iconImage.sprite = cut.icon;

        if (titleText != null)
            titleText.text = "Novo corte descoberto";

        if (subtitleText != null)
            subtitleText.text = cut.cutName + " • " + cut.decade;

        if (summaryText != null)
            summaryText.text = cut.historicalSummary;
    }

    public void Close()
    {
        if (root != null)
            root.SetActive(false);
    }
}