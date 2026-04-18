using System;
using UnityEngine;

public class PlayerEnergySystem : MonoBehaviour
{
    public static PlayerEnergySystem Instance { get; private set; }

    [Header("Energia")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float currentEnergy = 100f;
    [SerializeField] private float minEnergy = 0f;

    [Header("Fadiga acumulada")]
    [SerializeField] private float maxFatigue = 100f;
    [SerializeField] private float currentFatigue = 0f;
    [SerializeField] private float manualRestFatigueRecovery = 22f;

    [Header("Limites")]
    [SerializeField] private float lowEnergyThreshold = 35f;
    [SerializeField] private float criticalEnergyThreshold = 15f;
    [SerializeField] private float exhaustedThreshold = 10f;

    [Header("Barbearia")]
    [SerializeField] private Transform barbershopCenter;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float barbershopInsideRadius = 12f;

    [Header("Desgaste base")]
    [SerializeField] private float passiveDrainPerInBarbershopMinute = 0.010f;
    [SerializeField] private float passiveDrainPerOutsideMinute = 0.0035f;
    [SerializeField] private float fatigueGainPerInBarbershopMinute = 0.008f;

    [Header("Pressão da fila")]
    [SerializeField] private float queuePressureMultiplier = 0.03f;

    [Header("Recuperação fora da barbearia")]
    [SerializeField] private float minOutsideTimeBeforeRecovery = 15f;
    [SerializeField] private float baseRecoveryPerSecondOutside = 0.35f;
    [SerializeField] private float maxDistanceRecoveryMultiplier = 2.5f;

    [Header("Faixas de distância do bônus")]
    [SerializeField] private float distanceTier1 = 10f;
    [SerializeField] private float distanceTier2 = 25f;
    [SerializeField] private float distanceTier3 = 50f;
    [SerializeField] private float distanceTier4 = 90f;

    [Header("Horários de perda intensificada")]
    [SerializeField] private float insideAfterHoursDrainMultiplier = 2.2f;
    [SerializeField] private int outsideRestCallHour = 22;
    [SerializeField] private int outsideRestCallMinute = 0;
    [SerializeField] private float outsideAfterHoursDrainMultiplier = 1.4f;
    [SerializeField] private bool blockAllRecoveryAfterRestCallTime = true;

    [Header("Dívidas atrasadas")]
    [SerializeField] private int debtPenaltyStartThreshold = 1000;
    [SerializeField] private float baseDebtEnergyLossPercentPerDay = 0.01f;
    [SerializeField] private float extraDebtLossPercentPerOverdueDay = 0.003f;
    [SerializeField] private int maxDebtPenaltyDaysForScaling = 20;

    public float CurrentEnergy => currentEnergy;
    public float MaxEnergy => maxEnergy;
    public float CurrentFatigue => currentFatigue;
    public float MaxFatigue => maxFatigue;
    public float EnergyNormalized => maxEnergy <= 0f ? 0f : currentEnergy / maxEnergy;
    public float LowEnergyThreshold => lowEnergyThreshold;
    public float CriticalEnergyThreshold => criticalEnergyThreshold;
    public float ExhaustedThreshold => exhaustedThreshold;

    public bool IsInsideBarbershop { get; private set; }
    public float CurrentDistanceFromBarbershop { get; private set; }
    public float CurrentRecoveryMultiplier { get; private set; } = 1f;
    public float CurrentOutsideTime { get; private set; }
    public int ConsecutiveDebtPenaltyDays { get; private set; }
    public bool IsDebtPenaltyActive { get; private set; }
    public bool IsRecoveryBlockedByRestTime { get; private set; }
    public bool IsCollapsed { get; private set; }
    public string CurrentRecoveryBonusText { get; private set; } = "";
    public string CurrentDebtPenaltyText { get; private set; } = "";

    public event Action<float> OnEnergyChanged;
    public event Action OnEnergyContextChanged;
    public event Action OnEnergyDepleted;

    private float contextNotifyTimer;
    private bool depletionEventRaised;

    private int OutsideRestCallTotalMinutes => outsideRestCallHour * 60 + outsideRestCallMinute;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentEnergy = Mathf.Clamp(currentEnergy, minEnergy, maxEnergy);
        currentFatigue = Mathf.Clamp(currentFatigue, 0f, maxFatigue);
    }

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        RefreshLocationState();
        NotifyEnergyChanged();
        NotifyContextChanged();
    }

    private void Update()
    {
        RefreshLocationState();

        if (!IsCollapsed)
            ApplyPassiveEnergyLogic();

        contextNotifyTimer += Time.deltaTime;
        if (contextNotifyTimer >= 0.2f)
        {
            contextNotifyTimer = 0f;
            NotifyContextChanged();
        }
    }

    public bool CanStartService()
    {
        return !IsCollapsed && currentEnergy >= exhaustedThreshold;
    }

    public void ConsumeEnergy(float amount)
    {
        if (amount <= 0f || IsCollapsed)
            return;

        float oldValue = currentEnergy;
        currentEnergy = Mathf.Clamp(currentEnergy - amount, minEnergy, maxEnergy);

        if (!Mathf.Approximately(oldValue, currentEnergy))
        {
            NotifyEnergyChanged();
            CheckForDepletion();
        }
    }

    public void AddFatigue(float amount)
    {
        if (amount <= 0f)
            return;

        float oldValue = currentFatigue;
        currentFatigue = Mathf.Clamp(currentFatigue + amount, 0f, maxFatigue);

        if (!Mathf.Approximately(oldValue, currentFatigue))
            NotifyContextChanged();
    }

    public void RecoverEnergy(float amount)
    {
        if (amount <= 0f)
            return;

        float oldValue = currentEnergy;
        currentEnergy = Mathf.Clamp(currentEnergy + amount, minEnergy, maxEnergy);

        if (!Mathf.Approximately(oldValue, currentEnergy))
        {
            NotifyEnergyChanged();

            if (currentEnergy > 0f)
                depletionEventRaised = false;
        }
    }

    public void RecoverFatigue(float amount)
    {
        if (amount <= 0f)
            return;

        float oldValue = currentFatigue;
        currentFatigue = Mathf.Clamp(currentFatigue - amount, 0f, maxFatigue);

        if (!Mathf.Approximately(oldValue, currentFatigue))
            NotifyContextChanged();
    }

    public void ApplyCollapseRecovery(float recoveredEnergy)
    {
        IsCollapsed = false;
        RecoverEnergy(recoveredEnergy);
        RecoverFatigue(manualRestFatigueRecovery * 0.35f);
        NotifyContextChanged();
    }

    public void SetCollapsedState(bool value)
    {
        IsCollapsed = value;
        NotifyContextChanged();
    }

    public void ApplyManualRestRecovery()
    {
        RecoverEnergy(18f);
        RecoverFatigue(manualRestFatigueRecovery);
        ApplyDebtPenaltyForNewDay();
        NotifyContextChanged();
    }

    public float GetServiceTimeMultiplier()
    {
        float normalized = EnergyNormalized;
        float fatigueFactor = currentFatigue / maxFatigue;

        float multiplier = 1f;

        if (normalized <= 0.10f) multiplier += 0.45f;
        else if (normalized <= 0.20f) multiplier += 0.30f;
        else if (normalized <= 0.35f) multiplier += 0.20f;
        else if (normalized <= 0.55f) multiplier += 0.10f;

        multiplier += fatigueFactor * 0.15f;

        return Mathf.Max(1f, multiplier);
    }

    public float GetServiceQualityPenalty()
    {
        float normalized = EnergyNormalized;
        float fatigueFactor = currentFatigue / maxFatigue;

        float penalty = 0f;

        if (normalized <= 0.10f) penalty += 1.4f;
        else if (normalized <= 0.20f) penalty += 1.0f;
        else if (normalized <= 0.35f) penalty += 0.6f;
        else if (normalized <= 0.55f) penalty += 0.25f;

        penalty += fatigueFactor * 0.5f;

        return penalty;
    }

    public bool IsLowEnergy() => currentEnergy <= lowEnergyThreshold;
    public bool IsCriticalEnergy() => currentEnergy <= criticalEnergyThreshold;
    public bool IsExhausted() => currentEnergy < exhaustedThreshold;

    private void ApplyPassiveEnergyLogic()
    {
        if (playerTransform == null || GameTimeSystem.Instance == null)
            return;

        int currentMinutes = GameTimeSystem.Instance.CurrentTotalMinutes;
        bool afterBarbershopClosing = currentMinutes >= GameTimeSystem.Instance.ClosingTotalMinutes;
        bool afterOutsideRestCall = currentMinutes >= OutsideRestCallTotalMinutes;

        if (IsInsideBarbershop)
        {
            CurrentOutsideTime = 0f;
            CurrentRecoveryMultiplier = 1f;
            CurrentRecoveryBonusText = "";
            IsRecoveryBlockedByRestTime = afterBarbershopClosing && blockAllRecoveryAfterRestCallTime;

            float queueFactor = 1f;
            if (BarberQueueSystem.Instance != null)
            {
                int waitingCount = BarberQueueSystem.Instance.GetWaitingCount();
                queueFactor += waitingCount * queuePressureMultiplier;
            }

            float drainMultiplier = afterBarbershopClosing ? insideAfterHoursDrainMultiplier : 1f;

            float passiveDrain = passiveDrainPerInBarbershopMinute * Time.deltaTime * 60f * queueFactor * drainMultiplier;
            float passiveFatigue = fatigueGainPerInBarbershopMinute * Time.deltaTime * 60f * queueFactor * drainMultiplier;

            ConsumeEnergy(passiveDrain);
            AddFatigue(passiveFatigue);
        }
        else
        {
            CurrentOutsideTime += Time.deltaTime;
            CurrentRecoveryMultiplier = EvaluateDistanceRecoveryMultiplier(CurrentDistanceFromBarbershop);
            CurrentRecoveryBonusText = $"Bônus de recuperação {CurrentRecoveryMultiplier:0.0}x";

            bool recoveryBlocked = afterOutsideRestCall && blockAllRecoveryAfterRestCallTime;
            IsRecoveryBlockedByRestTime = recoveryBlocked;

            float outsideDrainMultiplier = afterOutsideRestCall ? outsideAfterHoursDrainMultiplier : 1f;
            float outsideDrain = passiveDrainPerOutsideMinute * Time.deltaTime * 60f * outsideDrainMultiplier;

            ConsumeEnergy(outsideDrain);

            if (!recoveryBlocked && CurrentOutsideTime >= minOutsideTimeBeforeRecovery)
            {
                float fatigueReductionFactor = 1f - ((currentFatigue / maxFatigue) * 0.35f);
                fatigueReductionFactor = Mathf.Clamp(fatigueReductionFactor, 0.5f, 1f);

                float recovery = baseRecoveryPerSecondOutside * CurrentRecoveryMultiplier * fatigueReductionFactor * Time.deltaTime;
                RecoverEnergy(recovery);

                float fatigueRecovery = 0.08f * CurrentRecoveryMultiplier * Time.deltaTime;
                RecoverFatigue(fatigueRecovery);
            }
        }
    }

    private float EvaluateDistanceRecoveryMultiplier(float distance)
    {
        if (distance < distanceTier1) return 1f;
        if (distance < distanceTier2) return 1.2f;
        if (distance < distanceTier3) return 1.5f;
        if (distance < distanceTier4) return 2f;
        return maxDistanceRecoveryMultiplier;
    }

    private void RefreshLocationState()
    {
        if (playerTransform == null || barbershopCenter == null)
        {
            IsInsideBarbershop = true;
            CurrentDistanceFromBarbershop = 0f;
            return;
        }

        CurrentDistanceFromBarbershop = Vector3.Distance(playerTransform.position, barbershopCenter.position);
        IsInsideBarbershop = CurrentDistanceFromBarbershop <= barbershopInsideRadius;
    }

    private void ApplyDebtPenaltyForNewDay()
    {
        int overdueDebt = 0;

        if (FinanceManager.Instance != null)
            overdueDebt = FinanceManager.Instance.GetOverdueDebtTotal();

        if (overdueDebt > debtPenaltyStartThreshold)
        {
            IsDebtPenaltyActive = true;
            ConsecutiveDebtPenaltyDays++;

            int effectiveDays = Mathf.Min(ConsecutiveDebtPenaltyDays, maxDebtPenaltyDaysForScaling);
            float percentLoss = baseDebtEnergyLossPercentPerDay + (extraDebtLossPercentPerOverdueDay * (effectiveDays - 1));
            float energyLoss = maxEnergy * percentLoss;

            ConsumeEnergy(energyLoss);
            CurrentDebtPenaltyText = "Perda de disposição por dívidas em atraso";
        }
        else
        {
            IsDebtPenaltyActive = false;
            ConsecutiveDebtPenaltyDays = 0;
            CurrentDebtPenaltyText = "";
        }
    }

    private void CheckForDepletion()
    {
        if (currentEnergy > 0f)
            return;

        if (depletionEventRaised)
            return;

        depletionEventRaised = true;
        IsCollapsed = true;
        NotifyContextChanged();
        OnEnergyDepleted?.Invoke();
    }

    private void NotifyEnergyChanged()
    {
        OnEnergyChanged?.Invoke(currentEnergy);
    }

    private void NotifyContextChanged()
    {
        OnEnergyContextChanged?.Invoke();
    }
}