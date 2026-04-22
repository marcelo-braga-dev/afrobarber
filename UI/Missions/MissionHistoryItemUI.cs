using TMPro;
using UnityEngine;

public class MissionHistoryItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text missionTitleText;
    [SerializeField] private TMP_Text metaText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text dateText;

    public void Setup(MissionHistoryEntry entry)
    {
        if (entry == null)
            return;

        if (missionTitleText != null)
            missionTitleText.text = entry.missionTitle;

        if (metaText != null)
            metaText.text = $"{entry.tierLabel} · Meta {entry.tierTarget}";

        if (rewardText != null)
            rewardText.text = entry.rewardSummary;

        if (dateText != null)
            dateText.text = FormatDate(entry.unlockedAtIsoUtc);
    }

    private string FormatDate(string isoUtc)
    {
        if (!System.DateTime.TryParse(isoUtc, out System.DateTime date))
            return "Sem data";

        return date.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
    }
}
