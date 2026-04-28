using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class MissionSystem : MonoBehaviour
{
    public static MissionSystem Instance { get; private set; }

    [Header("Configuração")]
    [SerializeField] private List<MissionDefinition> missions = new List<MissionDefinition>();
    [SerializeField] private int maxHistoryEntries = 200;
    [SerializeField] private string saveKey = "AFROBARBER_MISSION_SYSTEM_V1";

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    [Header("Eventos")]
    public UnityEvent OnMissionDataChanged;

    private readonly Dictionary<string, MissionProgressState> missionStateById = new Dictionary<string, MissionProgressState>();
    private readonly List<MissionHistoryEntry> history = new List<MissionHistoryEntry>();
    private readonly Dictionary<string, int> cutCounters = new Dictionary<string, int>();

    private MissionStatsState stats = new MissionStatsState();

    public IReadOnlyList<MissionDefinition> Missions => missions;
    public IReadOnlyList<MissionHistoryEntry> History => history;
    public MissionStatsState Stats => stats;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (OnMissionDataChanged == null)
            OnMissionDataChanged = new UnityEvent();

        PersistentGameObject.MakePersistent(gameObject);
        Load();
        EnsureMissionIds();
        RebuildCutCounters();

        if (ValidatePeriodResets())
            Save();
    }

    public void RegisterServiceCompleted(ClientRequestData request, int earnedMoney, float serviceMinutes)
    {
        stats.totalClientsServed++;
        stats.totalRevenue += Mathf.Max(0, earnedMoney);
        stats.totalServiceMinutes += Mathf.Max(0f, serviceMinutes);

        if (request != null && !string.IsNullOrWhiteSpace(request.afroCutId))
        {
            int current = 0;
            cutCounters.TryGetValue(request.afroCutId, out current);
            cutCounters[request.afroCutId] = current + 1;
        }

        Save();
        OnMissionDataChanged?.Invoke();

        if (debugLogs)
        {
            Debug.Log($"[MissionSystem] Serviço registrado | Clientes={stats.totalClientsServed} Receita={stats.totalRevenue} Tempo={stats.totalServiceMinutes:0.0}");
        }
    }

    public bool CanClaimTier(MissionDefinition mission)
    {
        if (mission == null || !IsMissionActive(mission))
            return false;

        MissionProgressState state = GetOrCreateState(mission);

        if (mission.tiers == null || mission.tiers.Count == 0)
            return false;

        if (state.claimedTierCount >= mission.tiers.Count)
            return false;

        int currentValue = GetMissionCurrentValue(mission);
        int target = Mathf.Max(1, mission.tiers[state.claimedTierCount].targetValue);

        return currentValue >= target;
    }

    public bool ClaimCurrentTier(MissionDefinition mission)
    {
        if (!CanClaimTier(mission))
            return false;

        MissionProgressState state = GetOrCreateState(mission);
        MissionTierDefinition tier = mission.tiers[state.claimedTierCount];

        GrantRewards(tier, mission);

        state.claimedTierCount++;

        MissionHistoryEntry entry = new MissionHistoryEntry
        {
            missionId = mission.missionId,
            missionTitle = mission.title,
            tierLabel = string.IsNullOrWhiteSpace(tier.tierLabel) ? $"Meta {state.claimedTierCount}" : tier.tierLabel,
            tierTarget = Mathf.Max(1, tier.targetValue),
            rewardSummary = BuildRewardSummary(tier),
            unlockedAtIsoUtc = DateTime.UtcNow.ToString("O"),
            claimed = true
        };

        history.Add(entry);
        TrimHistory();

        Save();
        OnMissionDataChanged?.Invoke();

        if (debugLogs)
            Debug.Log($"[MissionSystem] Recompensa coletada | Missão={mission.title} Tier={entry.tierLabel}");

        return true;
    }

    public int GetMissionCurrentValue(MissionDefinition mission)
    {
        if (mission == null)
            return 0;

        switch (mission.metricType)
        {
            case MissionMetricType.ClientsServed:
                return stats.totalClientsServed;

            case MissionMetricType.TotalRevenue:
                return stats.totalRevenue;

            case MissionMetricType.TotalServiceMinutes:
                return Mathf.FloorToInt(stats.totalServiceMinutes);

            case MissionMetricType.SpecificCutCompleted:
                if (string.IsNullOrWhiteSpace(mission.specificCutId))
                    return 0;

                int cutCount = 0;
                return cutCounters.TryGetValue(mission.specificCutId, out cutCount) ? cutCount : 0;

            default:
                return 0;
        }
    }

    public float GetCurrentTierProgress01(MissionDefinition mission)
    {
        if (mission == null || mission.tiers == null || mission.tiers.Count == 0)
            return 0f;

        MissionProgressState state = GetOrCreateState(mission);

        if (state.claimedTierCount >= mission.tiers.Count)
            return 1f;

        int current = GetMissionCurrentValue(mission);
        int nextTarget = Mathf.Max(1, mission.tiers[state.claimedTierCount].targetValue);
        int previousTarget = state.claimedTierCount <= 0
            ? 0
            : Mathf.Max(0, mission.tiers[state.claimedTierCount - 1].targetValue);

        int segmentCurrent = Mathf.Max(0, current - previousTarget);
        int segmentTarget = Mathf.Max(1, nextTarget - previousTarget);
        return Mathf.Clamp01((float)segmentCurrent / segmentTarget);
    }

    public string GetProgressLabel(MissionDefinition mission)
    {
        if (mission == null || mission.tiers == null || mission.tiers.Count == 0)
            return "0 / 0";

        MissionProgressState state = GetOrCreateState(mission);

        if (state.claimedTierCount >= mission.tiers.Count)
            return "Concluída";

        int current = GetMissionCurrentValue(mission);
        int target = Mathf.Max(1, mission.tiers[state.claimedTierCount].targetValue);
        return $"{current} / {target}";
    }

    public int GetActiveTierIndex(MissionDefinition mission)
    {
        if (mission == null || mission.tiers == null || mission.tiers.Count == 0)
            return -1;

        MissionProgressState state = GetOrCreateState(mission);

        if (state.claimedTierCount >= mission.tiers.Count)
            return mission.tiers.Count - 1;

        return Mathf.Clamp(state.claimedTierCount, 0, mission.tiers.Count - 1);
    }

    public string GetActiveTierRewardSummary(MissionDefinition mission)
    {
        int tierIndex = GetActiveTierIndex(mission);

        if (tierIndex < 0 || mission == null || mission.tiers == null || tierIndex >= mission.tiers.Count)
            return "Sem recompensa";

        return BuildRewardSummary(mission.tiers[tierIndex]);
    }

    public bool IsMissionActive(MissionDefinition mission)
    {
        if (mission == null)
            return false;

        DateTime now = DateTime.UtcNow;

        if (mission.scheduleType == MissionScheduleType.EventWindow || mission.isTemporary)
        {
            DateTime startsAt;
            DateTime endsAt;

            if (mission.TryGetWindow(out startsAt, out endsAt))
            {
                if (now < startsAt || now > endsAt)
                    return false;
            }
        }

        return true;
    }

    private void GrantRewards(MissionTierDefinition tier, MissionDefinition mission)
    {
        if (tier == null || tier.rewards == null)
            return;

        for (int i = 0; i < tier.rewards.Count; i++)
        {
            MissionRewardDefinition reward = tier.rewards[i];
            if (reward == null)
                continue;

            switch (reward.rewardType)
            {
                case MissionRewardType.Money:
                    if (FinanceManager.Instance != null)
                    {
                        FinanceManager.Instance.AddCashIncome(
                            $"Missão: {mission.title}",
                            $"Recompensa da meta {tier.tierLabel}",
                            Mathf.Max(0, reward.moneyAmount),
                            FinanceMovementOrigin.Other);
                    }
                    break;

                case MissionRewardType.Item:
                    if (InventoryManager.Instance != null && reward.itemReward != null)
                    {
                        int quantity = Mathf.Max(1, reward.itemAmount);
                        for (int amount = 0; amount < quantity; amount++)
                        {
                            InventoryManager.Instance.AddProduct(reward.itemReward);
                        }
                    }
                    break;
            }
        }
    }

    private string BuildRewardSummary(MissionTierDefinition tier)
    {
        if (tier == null || tier.rewards == null || tier.rewards.Count == 0)
            return "Sem recompensa";

        List<string> parts = new List<string>();

        for (int i = 0; i < tier.rewards.Count; i++)
        {
            MissionRewardDefinition reward = tier.rewards[i];
            if (reward == null)
                continue;

            if (reward.rewardType == MissionRewardType.Money)
            {
                parts.Add($"R$ {Mathf.Max(0, reward.moneyAmount)}");
                continue;
            }

            if (reward.rewardType == MissionRewardType.Item && reward.itemReward != null)
            {
                int amount = Mathf.Max(1, reward.itemAmount);
                string itemName = string.IsNullOrWhiteSpace(reward.itemReward.productName)
                    ? reward.itemReward.productId
                    : reward.itemReward.productName;

                parts.Add($"{itemName} x{amount}");
            }
        }

        return parts.Count > 0 ? string.Join(" + ", parts) : "Sem recompensa";
    }

    private MissionProgressState GetOrCreateState(MissionDefinition mission)
    {
        if (mission == null)
            return new MissionProgressState();

        string missionId = mission.missionId;

        if (string.IsNullOrWhiteSpace(missionId))
        {
            missionId = Guid.NewGuid().ToString("N");
            mission.missionId = missionId;
        }

        MissionProgressState state;
        if (!missionStateById.TryGetValue(missionId, out state))
        {
            state = new MissionProgressState
            {
                missionId = missionId,
                claimedTierCount = 0,
                periodAnchorTicks = DateTime.UtcNow.Ticks
            };

            missionStateById.Add(missionId, state);
        }

        return state;
    }

    private void EnsureMissionIds()
    {
        HashSet<string> ids = new HashSet<string>();

        for (int i = 0; i < missions.Count; i++)
        {
            MissionDefinition mission = missions[i];
            if (mission == null)
                continue;

            if (string.IsNullOrWhiteSpace(mission.missionId) || ids.Contains(mission.missionId))
                mission.missionId = Guid.NewGuid().ToString("N");

            ids.Add(mission.missionId);
            GetOrCreateState(mission);
        }

        Save();
    }

    private bool ValidatePeriodResets()
    {
        DateTime nowUtc = DateTime.UtcNow;
        bool hasReset = false;

        for (int i = 0; i < missions.Count; i++)
        {
            MissionDefinition mission = missions[i];
            if (mission == null)
                continue;

            if (mission.scheduleType == MissionScheduleType.Permanent || mission.scheduleType == MissionScheduleType.EventWindow)
                continue;

            MissionProgressState state = GetOrCreateState(mission);
            long anchorTicks = state.periodAnchorTicks > 0 ? state.periodAnchorTicks : nowUtc.Ticks;
            DateTime anchor = new DateTime(anchorTicks, DateTimeKind.Utc);
            bool shouldReset = false;

            if (mission.scheduleType == MissionScheduleType.Daily)
                shouldReset = nowUtc.Date > anchor.Date;
            else if (mission.scheduleType == MissionScheduleType.Weekly)
                shouldReset = (nowUtc - anchor).TotalDays >= 7d;

            if (!shouldReset)
                continue;

            state.claimedTierCount = 0;
            state.periodAnchorTicks = nowUtc.Ticks;
            hasReset = true;
        }

        return hasReset;
    }

    private void TrimHistory()
    {
        if (history.Count <= maxHistoryEntries)
            return;

        int removeCount = history.Count - maxHistoryEntries;
        history.RemoveRange(0, removeCount);
    }

    private void Save()
    {
        stats.cutCounts = cutCounters
            .Select(pair => new StringIntPair { key = pair.Key, value = pair.Value })
            .ToList();

        MissionSaveData save = new MissionSaveData
        {
            stats = stats,
            missions = missionStateById.Values.ToList(),
            history = history
        };

        string json = JsonUtility.ToJson(save);
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        missionStateById.Clear();
        history.Clear();
        cutCounters.Clear();

        string json = PlayerPrefs.GetString(saveKey, string.Empty);

        if (string.IsNullOrWhiteSpace(json))
        {
            stats = new MissionStatsState();
            return;
        }

        MissionSaveData save = JsonUtility.FromJson<MissionSaveData>(json);

        if (save == null)
        {
            stats = new MissionStatsState();
            return;
        }

        stats = save.stats ?? new MissionStatsState();

        if (save.missions != null)
        {
            for (int i = 0; i < save.missions.Count; i++)
            {
                MissionProgressState state = save.missions[i];
                if (state == null || string.IsNullOrWhiteSpace(state.missionId))
                    continue;

                missionStateById[state.missionId] = state;
            }
        }

        if (save.history != null)
            history.AddRange(save.history);
    }

    private void RebuildCutCounters()
    {
        cutCounters.Clear();

        if (stats.cutCounts == null)
            return;

        for (int i = 0; i < stats.cutCounts.Count; i++)
        {
            StringIntPair pair = stats.cutCounts[i];

            if (pair == null || string.IsNullOrWhiteSpace(pair.key))
                continue;

            cutCounters[pair.key] = Mathf.Max(0, pair.value);
        }
    }
}