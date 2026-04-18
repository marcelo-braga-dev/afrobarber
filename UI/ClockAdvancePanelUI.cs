using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ClockAdvancePanelUI : MonoBehaviour
{
    public static ClockAdvancePanelUI Instance { get; private set; }

    [Header("Painel")]
    [SerializeField] private GameObject panelRoot;

    [Header("Relógio")]
    [SerializeField] private RectTransform hourHand;
    [SerializeField] private RectTransform minuteHand;

    [Header("Configuração de rotação")]
    [SerializeField] private bool invertRotation = true;
    [SerializeField] private float hourHandOffset = 0f;
    [SerializeField] private float minuteHandOffset = 0f;

    [Header("Animação")]
    [SerializeField] private float defaultAnimationDuration = 3f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Eventos")]
    public UnityEvent onAdvanceAnimationStarted;
    public UnityEvent onAdvanceAnimationFinished;

    private Coroutine currentAnimationRoutine;
    private bool isPlaying;

    public bool IsPlaying => isPlaying;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void ShowAdvanceFromCurrentTime(int minutesToAdvance)
    {
        if (GameTimeSystem.Instance == null)
        {
            Debug.LogWarning("ClockAdvancePanelUI: GameTimeSystem.Instance não encontrado.");
            return;
        }

        DateTime startTime = GameTimeSystem.Instance.DisplayedDateTime;
        DateTime endTime = startTime.AddMinutes(minutesToAdvance);

        PlayAdvance(startTime, endTime, defaultAnimationDuration);
    }

    public void ShowAdvance(DateTime startTime, DateTime endTime, float duration)
    {
        PlayAdvance(startTime, endTime, duration);
    }

    public void PlayAdvance(DateTime startTime, DateTime endTime, float duration)
    {
        if (currentAnimationRoutine != null)
        {
            StopCoroutine(currentAnimationRoutine);
            currentAnimationRoutine = null;
        }

        currentAnimationRoutine = StartCoroutine(AnimateClockRoutine(startTime, endTime, duration));
    }

    private IEnumerator AnimateClockRoutine(DateTime startTime, DateTime endTime, float duration)
    {
        isPlaying = true;
        onAdvanceAnimationStarted?.Invoke();

        if (panelRoot != null)
            panelRoot.SetActive(true);

        SetClockInstant(startTime);

        float elapsed = 0f;
        duration = Mathf.Max(0.01f, duration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalized = Mathf.Clamp01(elapsed / duration);
            float curved = animationCurve.Evaluate(normalized);

            DateTime lerpedTime = LerpDateTime(startTime, endTime, curved);
            SetClockInstant(lerpedTime);

            yield return null;
        }

        SetClockInstant(endTime);

        yield return new WaitForSeconds(0.2f);

        if (panelRoot != null)
            panelRoot.SetActive(false);

        isPlaying = false;
        currentAnimationRoutine = null;
        onAdvanceAnimationFinished?.Invoke();
    }

    public void HidePanel()
    {
        if (currentAnimationRoutine != null)
        {
            StopCoroutine(currentAnimationRoutine);
            currentAnimationRoutine = null;
        }

        isPlaying = false;

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void SetClockInstant(DateTime time)
    {
        if (hourHand == null || minuteHand == null)
            return;

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

    private DateTime LerpDateTime(DateTime from, DateTime to, float t)
    {
        long fromTicks = from.Ticks;
        long toTicks = to.Ticks;
        long lerpedTicks = (long)Mathf.Lerp(fromTicks, toTicks, t);
        return new DateTime(lerpedTicks);
    }
}