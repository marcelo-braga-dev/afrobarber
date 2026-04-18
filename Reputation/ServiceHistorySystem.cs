using System;
using System.Collections.Generic;
using UnityEngine;

public class ServiceHistorySystem : MonoBehaviour
{
    public static ServiceHistorySystem Instance { get; private set; }

    [Header("Histórico")]
    [SerializeField] private List<ServiceHistoryEntry> entries = new List<ServiceHistoryEntry>();

    public IReadOnlyList<ServiceHistoryEntry> Entries => entries;

    public event Action OnHistoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddEntry(ServiceHistoryEntry entry)
    {
        if (entry == null)
            return;

        entry.finalRating = Mathf.Clamp(entry.finalRating, 0f, 5f);
        entry.finalRating = Mathf.Round(entry.finalRating * 10f) / 10f;
        entry.receivedValue = Mathf.Max(0f, entry.receivedValue);

        entries.Add(entry);

        if (GlobalReputationSystem.Instance != null)
        {
            GlobalReputationSystem.Instance.AddRating(entry.finalRating);
        }

        NotifyHistoryChanged();
    }

    public void AddEntry(int day, float timeOfDayMinutes, string clientName, float receivedValue, float finalRating, string serviceName = "")
    {
        ServiceHistoryEntry entry = new ServiceHistoryEntry(
            day,
            timeOfDayMinutes,
            clientName,
            receivedValue,
            finalRating,
            serviceName
        );

        AddEntry(entry);
    }

    public void RemoveEntry(ServiceHistoryEntry entry)
    {
        if (entry == null)
            return;

        bool removed = entries.Remove(entry);

        if (!removed)
            return;

        if (GlobalReputationSystem.Instance != null)
        {
            GlobalReputationSystem.Instance.RemoveRating(entry.finalRating);
        }

        NotifyHistoryChanged();
    }

    public void ClearHistory()
    {
        entries.Clear();

        if (GlobalReputationSystem.Instance != null)
        {
            GlobalReputationSystem.Instance.ResetReputation();
        }

        NotifyHistoryChanged();
    }

    private void NotifyHistoryChanged()
    {
        OnHistoryChanged?.Invoke();
    }
}