using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "MissionDefinition",
    menuName = "AfroBarber/Missions/Mission Definition"
)]
public class MissionDefinition : ScriptableObject
{
    [Header("Identidade")]
    public string missionId = Guid.NewGuid().ToString("N");
    public string title = "Nova missão";

    [TextArea]
    public string description;

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

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(missionId))
            missionId = Guid.NewGuid().ToString("N");

        if (tiers == null)
            tiers = new List<MissionTierDefinition>();

        for (int i = 0; i < tiers.Count; i++)
        {
            if (tiers[i] == null)
                continue;

            tiers[i].targetValue = Mathf.Max(1, tiers[i].targetValue);

            if (string.IsNullOrWhiteSpace(tiers[i].tierLabel))
                tiers[i].tierLabel = $"Meta {i + 1}";

            if (tiers[i].rewards == null)
                tiers[i].rewards = new List<MissionRewardDefinition>();

            for (int rewardIndex = 0; rewardIndex < tiers[i].rewards.Count; rewardIndex++)
            {
                MissionRewardDefinition reward = tiers[i].rewards[rewardIndex];

                if (reward == null)
                    continue;

                reward.moneyAmount = Mathf.Max(0, reward.moneyAmount);
                reward.xpAmount = Mathf.Max(0, reward.xpAmount);
                reward.itemAmount = Mathf.Max(1, reward.itemAmount);
            }
        }
    }
#endif
}