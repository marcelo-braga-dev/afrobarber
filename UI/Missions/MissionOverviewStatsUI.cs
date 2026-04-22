using TMPro;
using UnityEngine;

public class MissionOverviewStatsUI : MonoBehaviour
{
    [SerializeField] private TMP_Text totalClientsText;
    [SerializeField] private TMP_Text totalRevenueText;
    [SerializeField] private TMP_Text totalHoursText;

    private void OnEnable()
    {
        if (MissionSystem.Instance != null)
            MissionSystem.Instance.OnMissionDataChanged.AddListener(Refresh);

        Refresh();
    }

    private void OnDisable()
    {
        if (MissionSystem.Instance != null)
            MissionSystem.Instance.OnMissionDataChanged.RemoveListener(Refresh);
    }

    public void Refresh()
    {
        if (MissionSystem.Instance == null)
            return;

        MissionStatsState stats = MissionSystem.Instance.Stats;

        if (totalClientsText != null)
            totalClientsText.text = $"Clientes atendidos: {stats.totalClientsServed}";

        if (totalRevenueText != null)
            totalRevenueText.text = $"Faturamento total: R$ {stats.totalRevenue}";

        if (totalHoursText != null)
        {
            float hours = stats.totalServiceMinutes / 60f;
            totalHoursText.text = $"Horas de atendimento: {hours:0.0}h";
        }
    }
}
