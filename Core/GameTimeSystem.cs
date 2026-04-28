using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameTimeSystem : MonoBehaviour
{
    public static GameTimeSystem Instance { get; private set; }

    [Header("Estado atual do tempo real do jogo")]
    [SerializeField] private int currentYear = 2026;
    [SerializeField] private int currentMonth = 4;
    [SerializeField] private int currentDay = 10;
    [SerializeField] private int currentHour = 8;
    [SerializeField] private int currentMinute = 0;

    [Header("Estado exibido na UI")]
    [SerializeField] private int displayedYear = 2026;
    [SerializeField] private int displayedMonth = 4;
    [SerializeField] private int displayedDay = 10;
    [SerializeField] private int displayedHour = 8;
    [SerializeField] private int displayedMinute = 0;

    [Header("Inicialização")]
    [SerializeField] private bool startWithRealDate = false;
    [SerializeField] private bool startWithRealHour = false;

    [Header("Horário de funcionamento da barbearia")]
    [SerializeField] private int openingHour = 8;
    [SerializeField] private int openingMinute = 0;
    [SerializeField] private int closingHour = 19;
    [SerializeField] private int closingMinute = 0;

    [Header("Horário sugerido para ir descansar quando estiver fora")]
    [SerializeField] private int cityRestCallHour = 22;
    [SerializeField] private int cityRestCallMinute = 0;

    [Header("Velocidade do tempo automático")]
    [SerializeField] private float gameMinutesPerRealSecond = 1f;

    [Header("Dias de trabalho")]
    [SerializeField] private bool monday = true;
    [SerializeField] private bool tuesday = true;
    [SerializeField] private bool wednesday = true;
    [SerializeField] private bool thursday = true;
    [SerializeField] private bool friday = true;
    [SerializeField] private bool saturday = true;
    [SerializeField] private bool sunday = false;

    [Header("Animação visual do relógio")]
    [SerializeField] private bool animateAddedTimeInUI = true;
    [SerializeField] private float addedTimeAnimationDuration = 3f;
    [SerializeField] private float minVisualStepDelay = 0.03f;

    [Header("Opções")]
    [SerializeField] private bool clampToOpeningTimeOnStart = true;
    [SerializeField] private bool pauseTime = false;

    [Header("Eventos Unity")]
    public UnityEvent onTimeChanged;
    public UnityEvent onDisplayedTimeChanged;
    public UnityEvent onDayChanged;
    public UnityEvent onWorkDayStarted;
    public UnityEvent onRestStarted;

    private float accumulatedRealSeconds = 0f;

    private readonly Queue<int> visualMinuteQueue = new Queue<int>();
    private Coroutine visualClockRoutine;
    private bool isAnimatingVisualClock;

    public event Action OnTimeChangedAction;
    public event Action OnDisplayedTimeChangedAction;
    public event Action OnDayChangedAction;
    public event Action OnWorkDayStartedAction;
    public event Action OnRestStartedAction;

    public DateTime CurrentDateTime => new DateTime(currentYear, currentMonth, currentDay, currentHour, currentMinute, 0);
    public DateTime DisplayedDateTime => new DateTime(displayedYear, displayedMonth, displayedDay, displayedHour, displayedMinute, 0);

    public int CurrentYear => currentYear;
    public int CurrentMonth => currentMonth;
    public int CurrentDay => currentDay;
    public int CurrentHour => currentHour;
    public int CurrentMinute => currentMinute;

    public int DisplayedYear => displayedYear;
    public int DisplayedMonth => displayedMonth;
    public int DisplayedDay => displayedDay;
    public int DisplayedHour => displayedHour;
    public int DisplayedMinute => displayedMinute;

    public string CurrentTimeText => CurrentDateTime.ToString("HH:mm");
    public string CurrentDateText => CurrentDateTime.ToString("dd/MM/yyyy");
    public string CurrentDayShortText => GetPortugueseDayShort(CurrentDateTime.DayOfWeek);
    public string FullFormattedText => $"{CurrentTimeText} {CurrentDayShortText} {CurrentDateText}";

    public string DisplayedTimeText => DisplayedDateTime.ToString("HH:mm");
    public string DisplayedDateText => DisplayedDateTime.ToString("dd/MM/yyyy");
    public string DisplayedDayShortText => GetPortugueseDayShort(DisplayedDateTime.DayOfWeek);
    public string DisplayedFullFormattedText => $"{DisplayedTimeText} {DisplayedDayShortText} {DisplayedDateText}";

    public int OpeningHour => openingHour;
    public int OpeningMinute => openingMinute;
    public int ClosingHour => closingHour;
    public int ClosingMinute => closingMinute;

    public int OpeningTotalMinutes => openingHour * 60 + openingMinute;
    public int ClosingTotalMinutes => closingHour * 60 + closingMinute;
    public int CityRestCallTotalMinutes => cityRestCallHour * 60 + cityRestCallMinute;
    public int CurrentTotalMinutes => currentHour * 60 + currentMinute;

    public bool IsWorkDay => IsConfiguredWorkDay(CurrentDateTime.DayOfWeek);
    public bool IsWithinBusinessHours => CurrentTotalMinutes >= OpeningTotalMinutes && CurrentTotalMinutes < ClosingTotalMinutes;
    public bool IsAfterBarbershopClosingTime => CurrentTotalMinutes >= ClosingTotalMinutes;
    public bool IsAfterCityRestCallTime => CurrentTotalMinutes >= CityRestCallTotalMinutes;
    public bool IsAnimatingVisualClock => isAnimatingVisualClock;

    public bool IsMondayWorkDay => monday;
    public bool IsTuesdayWorkDay => tuesday;
    public bool IsWednesdayWorkDay => wednesday;
    public bool IsThursdayWorkDay => thursday;
    public bool IsFridayWorkDay => friday;
    public bool IsSaturdayWorkDay => saturday;
    public bool IsSundayWorkDay => sunday;

    public float CurrentTimeOfDayMinutes => CurrentTotalMinutes;
    public float TotalMinutesElapsed => (float)(CurrentDateTime - DateTime.MinValue).TotalMinutes;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        PersistentGameObject.MakePersistent(gameObject);
    }

    private void Start()
    {
        InitializeDateTime();
        AdjustStartTimeIfNeeded();
        SyncDisplayedTimeToCurrent();
        NotifyTimeChanged();
        NotifyDisplayedTimeChanged();
    }

    private void Update()
    {
        if (pauseTime)
            return;

        AdvanceTimeAutomatically();
    }

    private void InitializeDateTime()
    {
        DateTime now = DateTime.Now;

        if (startWithRealDate)
        {
            currentYear = now.Year;
            currentMonth = now.Month;
            currentDay = now.Day;
        }

        if (startWithRealHour)
        {
            currentHour = now.Hour;
            currentMinute = now.Minute;
        }
    }

    private void AdjustStartTimeIfNeeded()
    {
        DateTime dt = CurrentDateTime;

        if (!IsConfiguredWorkDay(dt.DayOfWeek))
        {
            MoveToNextWorkDayAtOpening();
            return;
        }

        if (!clampToOpeningTimeOnStart)
            return;

        int totalMinutes = dt.Hour * 60 + dt.Minute;

        if (totalMinutes < OpeningTotalMinutes)
        {
            SetTime(openingHour, openingMinute, false);
        }
    }

    private void AdvanceTimeAutomatically()
    {
        if (gameMinutesPerRealSecond <= 0f)
            return;

        accumulatedRealSeconds += Time.deltaTime * gameMinutesPerRealSecond;

        if (accumulatedRealSeconds < 1f)
            return;

        int wholeMinutes = Mathf.FloorToInt(accumulatedRealSeconds);
        accumulatedRealSeconds -= wholeMinutes;

        AddMinutesInternal(wholeMinutes, false, true);
    }

    public void AddMinutes(int minutesToAdd)
    {
        AddMinutes(minutesToAdd, true);
    }

    public void AddMinutes(float minutesToAdd)
    {
        int roundedMinutes = Mathf.RoundToInt(minutesToAdd);

        if (roundedMinutes <= 0 && minutesToAdd > 0f)
            roundedMinutes = 1;

        AddMinutes(roundedMinutes, true);
    }

    public void AddMinutes(int minutesToAdd, bool animateUI)
    {
        AddMinutesInternal(minutesToAdd, animateUI, false);
    }

    private void AddMinutesInternal(int minutesToAdd, bool animateUI, bool isAutomaticAdvance)
    {
        if (minutesToAdd <= 0)
            return;

        DateTime oldDate = CurrentDateTime;
        DateTime newDate = oldDate.AddMinutes(minutesToAdd);

        ApplyDateTime(newDate);

        if (oldDate.Date != CurrentDateTime.Date)
        {
            onDayChanged?.Invoke();
            OnDayChangedAction?.Invoke();
        }

        NotifyTimeChanged();

        if (isAutomaticAdvance)
        {
            SyncDisplayedTimeToCurrent();
            NotifyDisplayedTimeChanged();
            return;
        }

        if (!animateAddedTimeInUI || !animateUI)
        {
            SyncDisplayedTimeToCurrent();
            NotifyDisplayedTimeChanged();
            return;
        }

        EnqueueVisualMinutes(minutesToAdd);
    }

    public void AddHours(int hoursToAdd)
    {
        if (hoursToAdd <= 0)
            return;

        AddMinutes(hoursToAdd * 60);
    }

    public void AddServiceTime(int durationMinutes)
    {
        AddMinutes(durationMinutes, true);
    }

    public void SetDate(int year, int month, int day)
    {
        DateTime newDate = new DateTime(year, month, day, currentHour, currentMinute, 0);
        ApplyDateTime(newDate);

        SyncDisplayedTimeToCurrent();
        onDayChanged?.Invoke();
        OnDayChangedAction?.Invoke();
        NotifyTimeChanged();
        NotifyDisplayedTimeChanged();
    }

    public void SetTime(int hour, int minute)
    {
        SetTime(hour, minute, true);
    }

    public void SetTime(int hour, int minute, bool syncDisplay)
    {
        hour = Mathf.Clamp(hour, 0, 23);
        minute = Mathf.Clamp(minute, 0, 59);

        currentHour = hour;
        currentMinute = minute;

        NotifyTimeChanged();

        if (syncDisplay)
        {
            SyncDisplayedTimeToCurrent();
            NotifyDisplayedTimeChanged();
        }
    }

    public void SetBusinessHours(int openHour, int openMinute, int closeHour, int closeMinute)
    {
        openingHour = Mathf.Clamp(openHour, 0, 23);
        openingMinute = Mathf.Clamp(openMinute, 0, 59);
        closingHour = Mathf.Clamp(closeHour, 0, 23);
        closingMinute = Mathf.Clamp(closeMinute, 0, 59);

        NotifyTimeChanged();
        NotifyDisplayedTimeChanged();
    }

    public void SetWorkDays(
        bool mondayValue,
        bool tuesdayValue,
        bool wednesdayValue,
        bool thursdayValue,
        bool fridayValue,
        bool saturdayValue,
        bool sundayValue)
    {
        monday = mondayValue;
        tuesday = tuesdayValue;
        wednesday = wednesdayValue;
        thursday = thursdayValue;
        friday = fridayValue;
        saturday = saturdayValue;
        sunday = sundayValue;

        NotifyTimeChanged();
        NotifyDisplayedTimeChanged();
    }

    public void SetPause(bool value)
    {
        pauseTime = value;
    }

    public void RestUntilNextWorkdayStart(bool animateUI = false)
    {
        DateTime oldDate = CurrentDateTime;

        DateTime target = CurrentDateTime;
        target = target.AddDays(1);

        for (int i = 0; i < 14; i++)
        {
            if (IsConfiguredWorkDay(target.DayOfWeek))
            {
                target = new DateTime(target.Year, target.Month, target.Day, openingHour, openingMinute, 0);
                break;
            }

            target = target.AddDays(1);
        }

        int totalMinutesToAdvance = Mathf.Max(1, Mathf.RoundToInt((float)(target - CurrentDateTime).TotalMinutes));

        onRestStarted?.Invoke();
        OnRestStartedAction?.Invoke();

        AddMinutesInternal(totalMinutesToAdvance, animateUI, false);

        if (oldDate.Date != CurrentDateTime.Date)
        {
            onWorkDayStarted?.Invoke();
            OnWorkDayStartedAction?.Invoke();
        }
    }

    public string FormatDateTime(DateTime dateTime)
    {
        return $"{dateTime:HH:mm} {GetPortugueseDayShort(dateTime.DayOfWeek)} {dateTime:dd/MM/yyyy}";
    }

    public string GetFormattedCurrentDateTime()
    {
        return FormatDateTime(CurrentDateTime);
    }

    public int GetMinutesSinceStartOfDay()
    {
        return CurrentTotalMinutes;
    }

    public bool IsWorkDayByDayIndex(int dayIndex)
    {
        DayOfWeek day = DayOfWeekFromIndex(dayIndex);
        return IsConfiguredWorkDay(day);
    }

    public bool IsMinuteInsideBusinessHours(int dayIndex, int minuteOfDay)
    {
        if (!IsWorkDayByDayIndex(dayIndex))
            return false;

        minuteOfDay = Mathf.Clamp(minuteOfDay, 0, (24 * 60) - 1);
        return minuteOfDay >= OpeningTotalMinutes && minuteOfDay < ClosingTotalMinutes;
    }

    private void EnqueueVisualMinutes(int minutesToAdd)
    {
        for (int i = 0; i < minutesToAdd; i++)
        {
            visualMinuteQueue.Enqueue(1);
        }

        if (visualClockRoutine == null)
        {
            visualClockRoutine = StartCoroutine(AnimateDisplayedClockRoutine());
        }
    }

    private IEnumerator AnimateDisplayedClockRoutine()
    {
        isAnimatingVisualClock = true;

        while (visualMinuteQueue.Count > 0)
        {
            int queueCountAtStart = visualMinuteQueue.Count;
            float delayPerMinute = addedTimeAnimationDuration / Mathf.Max(1, queueCountAtStart);
            delayPerMinute = Mathf.Max(minVisualStepDelay, delayPerMinute);

            visualMinuteQueue.Dequeue();

            DateTime newDisplayedDate = DisplayedDateTime.AddMinutes(1);
            ApplyDisplayedDateTime(newDisplayedDate);
            NotifyDisplayedTimeChanged();

            yield return new WaitForSeconds(delayPerMinute);
        }

        SyncDisplayedTimeToCurrent();
        NotifyDisplayedTimeChanged();

        isAnimatingVisualClock = false;
        visualClockRoutine = null;
    }

    public void ForceSyncDisplayedClock()
    {
        visualMinuteQueue.Clear();

        if (visualClockRoutine != null)
        {
            StopCoroutine(visualClockRoutine);
            visualClockRoutine = null;
        }

        isAnimatingVisualClock = false;
        SyncDisplayedTimeToCurrent();
        NotifyDisplayedTimeChanged();
    }

    private void MoveToNextWorkDayAtOpening()
    {
        DateTime dt = CurrentDateTime;

        for (int i = 0; i < 14; i++)
        {
            if (IsConfiguredWorkDay(dt.DayOfWeek))
            {
                dt = new DateTime(dt.Year, dt.Month, dt.Day, openingHour, openingMinute, 0);
                ApplyDateTime(dt);
                return;
            }

            dt = dt.AddDays(1);
        }

        Debug.LogWarning("Nenhum dia útil foi configurado no GameTimeSystem.");
    }

    private bool IsConfiguredWorkDay(DayOfWeek day)
    {
        switch (day)
        {
            case DayOfWeek.Monday: return monday;
            case DayOfWeek.Tuesday: return tuesday;
            case DayOfWeek.Wednesday: return wednesday;
            case DayOfWeek.Thursday: return thursday;
            case DayOfWeek.Friday: return friday;
            case DayOfWeek.Saturday: return saturday;
            case DayOfWeek.Sunday: return sunday;
            default: return false;
        }
    }

    private DayOfWeek DayOfWeekFromIndex(int dayIndex)
    {
        int normalizedDay = ((dayIndex % 7) + 7) % 7;

        switch (normalizedDay)
        {
            case 0: return DayOfWeek.Monday;
            case 1: return DayOfWeek.Tuesday;
            case 2: return DayOfWeek.Wednesday;
            case 3: return DayOfWeek.Thursday;
            case 4: return DayOfWeek.Friday;
            case 5: return DayOfWeek.Saturday;
            case 6: return DayOfWeek.Sunday;
            default: return DayOfWeek.Monday;
        }
    }

    private void ApplyDateTime(DateTime dateTime)
    {
        currentYear = dateTime.Year;
        currentMonth = dateTime.Month;
        currentDay = dateTime.Day;
        currentHour = dateTime.Hour;
        currentMinute = dateTime.Minute;
    }

    private void ApplyDisplayedDateTime(DateTime dateTime)
    {
        displayedYear = dateTime.Year;
        displayedMonth = dateTime.Month;
        displayedDay = dateTime.Day;
        displayedHour = dateTime.Hour;
        displayedMinute = dateTime.Minute;
    }

    private void SyncDisplayedTimeToCurrent()
    {
        displayedYear = currentYear;
        displayedMonth = currentMonth;
        displayedDay = currentDay;
        displayedHour = currentHour;
        displayedMinute = currentMinute;
    }

    private void NotifyTimeChanged()
    {
        onTimeChanged?.Invoke();
        OnTimeChangedAction?.Invoke();
    }

    private void NotifyDisplayedTimeChanged()
    {
        onDisplayedTimeChanged?.Invoke();
        OnDisplayedTimeChangedAction?.Invoke();
    }

    private string GetPortugueseDayShort(DayOfWeek day)
    {
        switch (day)
        {
            case DayOfWeek.Monday: return "Seg.";
            case DayOfWeek.Tuesday: return "Ter.";
            case DayOfWeek.Wednesday: return "Qua.";
            case DayOfWeek.Thursday: return "Qui.";
            case DayOfWeek.Friday: return "Sex.";
            case DayOfWeek.Saturday: return "Sáb.";
            case DayOfWeek.Sunday: return "Dom.";
            default: return "---";
        }
    }
}