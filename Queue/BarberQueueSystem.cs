using System;
using System.Collections.Generic;
using UnityEngine;

public class BarberQueueSystem : MonoBehaviour
{
    public static BarberQueueSystem Instance { get; private set; }

    [SerializeField] private List<ClientQueueData> waitingClients = new List<ClientQueueData>();

    public IReadOnlyList<ClientQueueData> WaitingClients => waitingClients;

    public event Action OnQueueChanged;

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

    public void AddClientToQueue(ClientNPC clientNPC, string clientName, float maxPatienceMinutes)
    {
        if (clientNPC == null)
        {
            Debug.LogWarning("[BarberQueueSystem] Tentativa de adicionar cliente null.");
            return;
        }

        if (IsClientInQueue(clientNPC))
        {
            Debug.LogWarning($"[BarberQueueSystem] Cliente {clientName} já está na fila.");
            return;
        }

        float currentGameMinutes = GetCurrentGameMinutes();

        ClientQueueData newData = new ClientQueueData(
            clientNPC,
            clientName,
            currentGameMinutes,
            maxPatienceMinutes
        );

        waitingClients.Add(newData);

        Debug.Log($"[BarberQueueSystem] Cliente adicionado: {clientName}. Total na fila: {GetWaitingCount()}");

        NotifyQueueChanged();
    }

    public void RemoveClientFromQueue(ClientNPC clientNPC)
    {
        if (clientNPC == null)
            return;

        int removedCount = waitingClients.RemoveAll(x => x.clientNPC == clientNPC);

        if (removedCount > 0)
        {
            Debug.Log($"[BarberQueueSystem] Cliente removido. Total na fila: {GetWaitingCount()}");
            NotifyQueueChanged();
        }
    }

    public void MarkClientAsBeingServed(ClientNPC clientNPC, bool value)
    {
        ClientQueueData data = GetClientData(clientNPC);
        if (data == null)
            return;

        data.isBeingServed = value;
        NotifyQueueChanged();
    }

    public ClientQueueData GetClientData(ClientNPC clientNPC)
    {
        return waitingClients.Find(x => x.clientNPC == clientNPC);
    }

    public bool IsClientInQueue(ClientNPC clientNPC)
    {
        return waitingClients.Exists(x => x.clientNPC == clientNPC);
    }

    public int GetWaitingCount()
    {
        int count = 0;

        for (int i = 0; i < waitingClients.Count; i++)
        {
            if (!waitingClients[i].isBeingServed)
                count++;
        }

        return count;
    }

    public bool TryGetNextWaitingClient(out ClientNPC clientNPC)
    {
        clientNPC = null;

        for (int i = 0; i < waitingClients.Count; i++)
        {
            ClientQueueData data = waitingClients[i];

            if (data == null || data.clientNPC == null || data.isBeingServed)
                continue;

            clientNPC = data.clientNPC;
            return true;
        }

        return false;
    }

    public List<ClientQueueData> GetQueueOrderedByArrival()
    {
        List<ClientQueueData> ordered = new List<ClientQueueData>(waitingClients);

        ordered.Sort((a, b) =>
        {
            return a.arrivalGameMinutes.CompareTo(b.arrivalGameMinutes);
        });

        return ordered;
    }

    public List<ClientQueueData> GetQueueOrderedByUrgency()
    {
        float currentGameMinutes = GetCurrentGameMinutes();
        List<ClientQueueData> ordered = new List<ClientQueueData>(waitingClients);

        ordered.Sort((a, b) =>
        {
            float urgencyA = a.GetPatiencePercent(currentGameMinutes);
            float urgencyB = b.GetPatiencePercent(currentGameMinutes);

            int urgencyCompare = urgencyB.CompareTo(urgencyA);
            if (urgencyCompare != 0)
                return urgencyCompare;

            return a.arrivalGameMinutes.CompareTo(b.arrivalGameMinutes);
        });

        return ordered;
    }

    public float GetCurrentGameMinutes()
    {
        if (GameTimeSystem.Instance == null)
            return 0f;

        return GameTimeSystem.Instance.TotalMinutesElapsed;
    }

    private void NotifyQueueChanged()
    {
        OnQueueChanged?.Invoke();
    }
}