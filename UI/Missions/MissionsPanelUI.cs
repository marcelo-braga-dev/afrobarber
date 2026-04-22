using System.Collections.Generic;
using UnityEngine;

public class MissionsPanelUI : MonoBehaviour
{
    [Header("Painel")]
    [SerializeField] private GameObject rootPanel;

    [Header("Missões")]
    [SerializeField] private Transform missionListParent;
    [SerializeField] private MissionProgressItemUI missionItemPrefab;

    [Header("Histórico")]
    [SerializeField] private Transform historyListParent;
    [SerializeField] private MissionHistoryItemUI historyItemPrefab;

    private readonly List<MissionProgressItemUI> missionItems = new List<MissionProgressItemUI>();
    private readonly List<MissionHistoryItemUI> historyItems = new List<MissionHistoryItemUI>();

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

    public void Toggle()
    {
        if (rootPanel == null)
            return;

        rootPanel.SetActive(!rootPanel.activeSelf);

        if (rootPanel.activeSelf)
            Refresh();
    }

    public void Refresh()
    {
        RefreshMissions();
        RefreshHistory();
    }

    private void RefreshMissions()
    {
        if (missionListParent == null || missionItemPrefab == null)
            return;

        ClearSpawned(missionItems);

        if (MissionSystem.Instance == null)
            return;

        IReadOnlyList<MissionDefinition> missions = MissionSystem.Instance.Missions;

        for (int i = 0; i < missions.Count; i++)
        {
            MissionDefinition mission = missions[i];

            if (mission == null || !MissionSystem.Instance.IsMissionActive(mission))
                continue;

            MissionProgressItemUI item = Instantiate(missionItemPrefab, missionListParent);
            item.Setup(mission);
            missionItems.Add(item);
        }
    }

    private void RefreshHistory()
    {
        if (historyListParent == null || historyItemPrefab == null)
            return;

        ClearSpawned(historyItems);

        if (MissionSystem.Instance == null)
            return;

        IReadOnlyList<MissionHistoryEntry> history = MissionSystem.Instance.History;

        for (int i = history.Count - 1; i >= 0; i--)
        {
            MissionHistoryItemUI item = Instantiate(historyItemPrefab, historyListParent);
            item.Setup(history[i]);
            historyItems.Add(item);
        }
    }

    private void ClearSpawned<T>(List<T> list) where T : MonoBehaviour
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != null)
                Destroy(list[i].gameObject);
        }

        list.Clear();
    }
}
