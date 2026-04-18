using TMPro;
using UnityEngine;

public class GameTimeUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text dayText;

    private void Start()
    {
        if (GameTimeSystem.Instance != null)
        {
            GameTimeSystem.Instance.onDisplayedTimeChanged.AddListener(UpdateUI);
            UpdateUI();
        }
    }

    private void OnDestroy()
    {
        if (GameTimeSystem.Instance != null)
        {
            GameTimeSystem.Instance.onDisplayedTimeChanged.RemoveListener(UpdateUI);
        }
    }

    private void UpdateUI()
    {
        if (timeText == null || GameTimeSystem.Instance == null)
            return;

        timeText.text = GameTimeSystem.Instance.DisplayedTimeText;
        dateText.text = GameTimeSystem.Instance.DisplayedDateText;
        dayText.text = GameTimeSystem.Instance.DisplayedDayShortText; 
    }
}