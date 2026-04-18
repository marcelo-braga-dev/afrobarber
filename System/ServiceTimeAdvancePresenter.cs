using System;
using UnityEngine;

public class ServiceTimeAdvancePresenter : MonoBehaviour
{
    public static ServiceTimeAdvancePresenter Instance { get; private set; }

    [SerializeField] private float defaultAdvanceAnimationDuration = 3f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ShowServiceAdvance(int serviceMinutes)
    {
        if (serviceMinutes <= 0)
            return;

        if (GameTimeSystem.Instance == null)
        {
            Debug.LogWarning("ServiceTimeAdvancePresenter: GameTimeSystem.Instance não encontrado.");
            return;
        }

        if (ClockAdvancePanelUI.Instance == null)
        {
            Debug.LogWarning("ServiceTimeAdvancePresenter: ClockAdvancePanelUI.Instance não encontrado.");
            return;
        }

        DateTime startTime = GameTimeSystem.Instance.DisplayedDateTime;
        DateTime endTime = startTime.AddMinutes(serviceMinutes);

        ClockAdvancePanelUI.Instance.ShowAdvance(startTime, endTime, defaultAdvanceAnimationDuration);
    }
}