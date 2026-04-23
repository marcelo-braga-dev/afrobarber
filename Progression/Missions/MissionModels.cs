using System;
using System.Collections.Generic;
using UnityEngine;

public enum MissionMetricType
{
    ClientsServed,
    TotalRevenue,
    TotalServiceMinutes,
    SpecificCutCompleted
}

public enum MissionScheduleType
{
    Permanent,
    Daily,
    Weekly,
    EventWindow
}

public enum MissionRewardType
{
    Money,
    Item,
    XP
}

[Serializable]
public class MissionRewardDefinition
{
    public MissionRewardType rewardType = MissionRewardType.Money;
    [Min(0)] public int moneyAmount;
    [Min(0)] public int xpAmount;
    public ProductData itemReward;
    [Min(1)] public int itemAmount = 1;
}

[Serializable]
public class MissionTierDefinition
{
    [Min(1)] public int targetValue = 1;
    public string tierLabel = "Meta";
    public List<MissionRewardDefinition> rewards = new List<MissionRewardDefinition>();
}

[CreateAssetMenu(fileName = "MissionDefinition", menuName = "Afrobarber/Missions/Mission Definition")]
public class MissionDefinition : ScriptableObject
{
    [Header("Identidade")]
    public string missionId = Guid.NewGuid().ToString("N");
    public string title = "Nova missão";
    [TextArea] public string description;

    [Header("Métrica")]
    public MissionMetricType metricType = MissionMetricType.ClientsServed;
    [Tooltip("Para missão por corte específico, preencher com request.afroCutId")]
    public string specificCutId;

    [Header("Agenda")]
    public MissionScheduleType scheduleType = MissionScheduleType.Permanent;
    public bool isTemporary;
    public string seasonTag;
    public string startsAtIsoUtc;
    public string endsAtIsoUtc;

    [Header("Progressão por metas")]
    public List<MissionTierDefinition> tiers = new List<MissionTierDefinition>();

    public bool TryGetWindow(out DateTime startsAtUtc, out DateTime endsAtUtc)
    {
        bool hasStart = DateTime.TryParse(startsAtIsoUtc, out startsAtUtc);
        bool hasEnd = DateTime.TryParse(endsAtIsoUtc, out endsAtUtc);

        if (!hasStart)
            startsAtUtc = DateTime.MinValue;

        if (!hasEnd)
            endsAtUtc = DateTime.MaxValue;

        return hasStart || hasEnd;
    }
}

[Serializable]
public class MissionProgressState
{
    public string missionId;
    public int claimedTierCount;
    public long periodAnchorTicks;
    public int baselineClientsServed;
    public int baselineRevenue;
    public float baselineServiceMinutes;
    public int baselineSpecificCutCompleted;
}

[Serializable]
public class MissionHistoryEntry
{
    public string missionId;
    public string missionTitle;
    public string tierLabel;
    public int tierTarget;
    public string rewardSummary;
    public string unlockedAtIsoUtc;
    public bool claimed;
}

[Serializable]
public class MissionStatsState
{
    public int totalClientsServed;
    public int totalRevenue;
    public float totalServiceMinutes;
    public List<StringIntPair> cutCounts = new List<StringIntPair>();
}

[Serializable]
public class StringIntPair
{
    public string key;
    public int value;
}

[Serializable]
public class MissionSaveData
{
    public MissionStatsState stats = new MissionStatsState();
    public List<MissionProgressState> missions = new List<MissionProgressState>();
    public List<MissionHistoryEntry> history = new List<MissionHistoryEntry>();
}
