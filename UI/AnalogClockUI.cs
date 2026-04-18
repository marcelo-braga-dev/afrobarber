using System;
using UnityEngine;

public class AnalogClockUI : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private RectTransform hourHand;
    [SerializeField] private RectTransform minuteHand;

    [Header("Configuração de rotação")]
    [SerializeField] private bool invertRotation = true;
    [SerializeField] private float hourHandOffset = 0f;
    [SerializeField] private float minuteHandOffset = 0f;

    [Header("Fonte do horário")]
    [SerializeField] private bool useDisplayedTime = true;

    [Header("Atualização")]
    [SerializeField] private bool updateEveryFrame = false;

    private void Start()
    {
        Subscribe();
        RefreshClock();
    }

    private void Update()
    {
        if (updateEveryFrame)
        {
            RefreshClock();
        }
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (GameTimeSystem.Instance == null)
            return;

        if (useDisplayedTime)
            GameTimeSystem.Instance.onDisplayedTimeChanged.AddListener(RefreshClock);
        else
            GameTimeSystem.Instance.onTimeChanged.AddListener(RefreshClock);
    }

    private void Unsubscribe()
    {
        if (GameTimeSystem.Instance == null)
            return;

        if (useDisplayedTime)
            GameTimeSystem.Instance.onDisplayedTimeChanged.RemoveListener(RefreshClock);
        else
            GameTimeSystem.Instance.onTimeChanged.RemoveListener(RefreshClock);
    }

    public void RefreshClock()
    {
        if (hourHand == null || minuteHand == null)
            return;

        DateTime time = GetSourceTime();

        float minuteValue = time.Minute;
        float hourValue = (time.Hour % 12) + (time.Minute / 60f);

        float minuteAngle = minuteValue * 6f;
        float hourAngle = hourValue * 30f;

        if (invertRotation)
        {
            minuteAngle = -minuteAngle;
            hourAngle = -hourAngle;
        }

        minuteAngle += minuteHandOffset;
        hourAngle += hourHandOffset;

        minuteHand.localRotation = Quaternion.Euler(0f, 0f, minuteAngle);
        hourHand.localRotation = Quaternion.Euler(0f, 0f, hourAngle);
    }

    private DateTime GetSourceTime()
    {
        if (GameTimeSystem.Instance == null)
            return DateTime.Now;

        return useDisplayedTime
            ? GameTimeSystem.Instance.DisplayedDateTime
            : GameTimeSystem.Instance.CurrentDateTime;
    }
}