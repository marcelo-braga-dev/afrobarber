using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EducationEncyclopediaItemUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private GameObject lockIcon;

    private AfroCutInfo currentCut;
    private bool currentUnlocked;
    private Action<AfroCutInfo, bool> onClick;

    public void Setup(AfroCutInfo cut, bool unlocked, Action<AfroCutInfo, bool> clickCallback)
    {
        currentCut = cut;
        currentUnlocked = unlocked;
        onClick = clickCallback;

        if (iconImage != null)
            iconImage.sprite = cut.icon;

        if (titleText != null)
            titleText.text = unlocked ? cut.cutName : "???";

        if (subtitleText != null)
            subtitleText.text = unlocked ? cut.decade : "Bloqueado";

        if (lockIcon != null)
            lockIcon.SetActive(!unlocked);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }
    }

    private void HandleClick()
    {
        onClick?.Invoke(currentCut, currentUnlocked);
    }
}