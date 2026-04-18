using TMPro;
using UnityEngine;

public class GameTimeDebugUI : MonoBehaviour
{
    [SerializeField] private TMP_Text clockText;

    private void Start()
    {
        UpdateClock();

        if (GameTimeSystem.Instance != null)
            GameTimeSystem.Instance.onTimeChanged.AddListener(UpdateClock);
    }

    private void OnDestroy()
    {
        if (GameTimeSystem.Instance != null)
            GameTimeSystem.Instance.onTimeChanged.RemoveListener(UpdateClock);
    }

    private void UpdateClock()
    {
        if (clockText == null || GameTimeSystem.Instance == null)
            return;

        clockText.text = GameTimeSystem.Instance.FullFormattedText;
    }
}