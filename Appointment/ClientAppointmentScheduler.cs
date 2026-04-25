using System;
using System.Collections.Generic;
using UnityEngine;

public class ClientAppointmentScheduler : MonoBehaviour
{
    public static ClientAppointmentScheduler Instance { get; private set; }

    [Header("Banco")]
    [SerializeField] private List<GameObject> clientPrefabs = new List<GameObject>();
    [SerializeField] private List<ClientRequestData> possibleRequests = new List<ClientRequestData>();

    [Header("Agenda")]
    [SerializeField] private List<ClientAppointmentData> appointments = new List<ClientAppointmentData>();

    [Header("Geração automática")]
    [SerializeField] private bool autoGenerateAppointments = true;
    [SerializeField] private float baseIntervalMinutes = 90f;
    [SerializeField] private float minIntervalMinutes = 20f;

    [Header("Overbooking")]
    [SerializeField] private bool allowOverbooking = true;
    [SerializeField] private int maxAppointmentsPerHour = 2;
    [SerializeField] private float overbookingChance = 0.25f;

    [Header("Atraso em cascata")]
    [SerializeField] private bool enableCascadeDelay = true;
    [SerializeField] private float delayPerWaitingClientMinutes = 8f;
    [SerializeField] private float maxCascadeDelayMinutes = 45f;

    [Header("Horário")]
    [SerializeField] private int openingHour = 8;
    [SerializeField] private int closingHour = 18;

    [Header("Performance")]
    [SerializeField] private float processIntervalSeconds = 1f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private float nextGenerationMinute = -1f;
    private float processTimer;

    public IReadOnlyList<ClientAppointmentData> Appointments => appointments;

    public event Action OnAppointmentsChanged;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializeNextGenerationTime();
        NotifyChanged();
    }

    private void Update()
    {
        if (GameTimeSystem.Instance == null)
            return;

        processTimer += Time.deltaTime;

        if (processTimer < processIntervalSeconds)
            return;

        processTimer = 0f;

        float currentMinutes = GameTimeSystem.Instance.TotalMinutesElapsed;

        if (nextGenerationMinute < 0f)
            InitializeNextGenerationTime();

        if (autoGenerateAppointments && currentMinutes >= nextGenerationMinute)
        {
            TryGenerateAppointment();
            ScheduleNextGeneration();
        }

        ProcessAppointments(currentMinutes);
    }

    private void InitializeNextGenerationTime()
    {
        if (GameTimeSystem.Instance == null)
            return;

        nextGenerationMinute = GameTimeSystem.Instance.TotalMinutesElapsed + GetCurrentIntervalMinutes();
    }

    private void ProcessAppointments(float currentMinutes)
    {
        for (int i = 0; i < appointments.Count; i++)
        {
            ClientAppointmentData appointment = appointments[i];

            if (appointment == null)
                continue;

            if (appointment.spawned || appointment.cancelled)
                continue;

            ApplyCascadeDelay(appointment);

            float spawnMinute = appointment.AppointmentTotalMinutes
                + appointment.cascadeDelayMinutes
                - EstimateWalkTimeMinutes(appointment.clientPrefab);

            if (currentMinutes >= spawnMinute)
                TrySpawnAppointment(appointment, currentMinutes);
        }
    }

    private void TrySpawnAppointment(ClientAppointmentData appointment, float currentMinutes)
    {
        if (appointment == null)
            return;

        if (!ClientSpawnerLocator.TryGet(out ClientSpawner spawner))
        {
            if (enableDebugLogs)
                Debug.LogWarning("[Agenda] ClientSpawner não encontrado na cena.");

            return;
        }

        bool success = spawner.TrySpawnScheduledClient(
            appointment.clientPrefab,
            appointment.requestData,
            out ClientNPC spawnedClient
        );

        if (!success)
        {
            appointment.status = ClientAppointmentStatus.WaitingToSpawn;
            NotifyChanged();
            return;
        }

        appointment.spawned = true;
        appointment.spawnedClient = spawnedClient;

        if (appointment.status != ClientAppointmentStatus.Overbooked)
            appointment.status = ClientAppointmentStatus.Spawned;

        SendArrivalDialogue(appointment, currentMinutes);
        NotifyChanged();
    }

    private void SendArrivalDialogue(ClientAppointmentData appointment, float currentMinutes)
    {
        if (appointment == null || appointment.clientPrefab == null)
            return;

        ClientAppointmentProfile profile = appointment.clientPrefab.GetComponent<ClientAppointmentProfile>();

        string clientName = appointment.clientPrefab.name;
        string message;

        float realArrivalDelay = currentMinutes - appointment.AppointmentTotalMinutes;

        if (appointment.status == ClientAppointmentStatus.Overbooked)
        {
            message = profile != null
                ? profile.overbookingMessage
                : "Ainda dá tempo de me atender?";
        }
        else if (realArrivalDelay > 3f)
        {
            message = profile != null
                ? profile.lateMessage
                : "Me desculpe! Demorei um pouco para chegar.";
        }
        else
        {
            message = profile != null
                ? profile.onTimeMessage
                : "Cheguei!";
        }

        if (ClientAppointmentDialogueBridge.Instance != null)
            ClientAppointmentDialogueBridge.Instance.SendClientMessage(clientName, message);
    }

    private void ApplyCascadeDelay(ClientAppointmentData appointment)
    {
        if (!enableCascadeDelay)
            return;

        if (appointment == null)
            return;

        if (BarberQueueSystem.Instance == null)
            return;

        int waitingCount = BarberQueueSystem.Instance.GetWaitingCount();

        float delay = waitingCount * delayPerWaitingClientMinutes;
        delay = Mathf.Clamp(delay, 0f, maxCascadeDelayMinutes);

        appointment.cascadeDelayMinutes = delay;

        if (delay > 0f && appointment.status == ClientAppointmentStatus.Scheduled)
            appointment.status = ClientAppointmentStatus.DelayedByQueue;

        if (delay >= 15f && !appointment.warningMessageSent)
        {
            appointment.warningMessageSent = true;

            ClientAppointmentProfile profile = appointment.clientPrefab != null
                ? appointment.clientPrefab.GetComponent<ClientAppointmentProfile>()
                : null;

            string message = profile != null
                ? profile.cascadeDelayMessage
                : "Ainda vai demorar muito?";

            string clientName = appointment.clientPrefab != null
                ? appointment.clientPrefab.name
                : "Cliente";

            if (ClientAppointmentDialogueBridge.Instance != null)
                ClientAppointmentDialogueBridge.Instance.SendClientMessage(clientName, message);
        }
    }

    private float EstimateWalkTimeMinutes(GameObject prefab)
    {
        ClientAppointmentProfile profile = prefab != null
            ? prefab.GetComponent<ClientAppointmentProfile>()
            : null;

        float speed = profile != null ? profile.averageWalkSpeed : 2.5f;

        return Mathf.Clamp(6f / speed, 1f, 15f);
    }

    private void TryGenerateAppointment()
    {
        if (GameTimeSystem.Instance == null)
            return;

        if (!IsInsideBusinessHours())
            return;

        GameObject prefab = PickRandomClientPrefab();

        if (prefab == null)
        {
            Debug.LogWarning("[Agenda] Nenhum prefab de cliente configurado.");
            return;
        }

        ClientRequestData request = PickRandomRequest();

        float currentMinutes = GameTimeSystem.Instance.TotalMinutesElapsed;
        float targetMinute = currentMinutes + GetCurrentIntervalMinutes();

        int dayIndex = Mathf.FloorToInt(targetMinute / 1440f);
        int minuteOfDay = Mathf.FloorToInt(targetMinute % 1440f);

        int hour = minuteOfDay / 60;
        int minute = minuteOfDay % 60;

        if (hour < openingHour || hour >= closingHour)
            return;

        bool hourFull = CountAppointmentsAt(dayIndex, hour) >= maxAppointmentsPerHour;

        if (hourFull && (!allowOverbooking || UnityEngine.Random.value > overbookingChance))
            return;

        ClientAppointmentData appointment = new ClientAppointmentData
        {
            id = Guid.NewGuid().ToString(),
            clientPrefab = prefab,
            requestData = request,
            dayIndex = dayIndex,
            hour = hour,
            minute = minute,
            status = hourFull ? ClientAppointmentStatus.Overbooked : ClientAppointmentStatus.Scheduled,
            spawned = false,
            cancelled = false
        };

        appointments.Add(appointment);

        if (enableDebugLogs)
            Debug.Log($"[Agenda] Novo horário criado: {prefab.name} às {appointment.TimeText}");

        NotifyChanged();
    }

    private int CountAppointmentsAt(int dayIndex, int hour)
    {
        int count = 0;

        for (int i = 0; i < appointments.Count; i++)
        {
            ClientAppointmentData item = appointments[i];

            if (item == null)
                continue;

            if (item.dayIndex == dayIndex && item.hour == hour && !item.cancelled)
                count++;
        }

        return count;
    }

    private float GetCurrentIntervalMinutes()
    {
        float reputationFactor = 0f;

        if (BarbershopRatingManager.Instance != null)
            reputationFactor = Mathf.Clamp01(BarbershopRatingManager.Instance.GlobalRating / 5f);

        return Mathf.Lerp(baseIntervalMinutes, minIntervalMinutes, reputationFactor);
    }

    private void ScheduleNextGeneration()
    {
        if (GameTimeSystem.Instance == null)
            return;

        nextGenerationMinute = GameTimeSystem.Instance.TotalMinutesElapsed + GetCurrentIntervalMinutes();
    }

    private bool IsInsideBusinessHours()
    {
        if (GameTimeSystem.Instance == null)
            return true;

        int hour = Mathf.FloorToInt((GameTimeSystem.Instance.TotalMinutesElapsed % 1440f) / 60f);

        return hour >= openingHour && hour < closingHour;
    }

    private GameObject PickRandomClientPrefab()
    {
        if (clientPrefabs == null || clientPrefabs.Count == 0)
            return null;

        return clientPrefabs[UnityEngine.Random.Range(0, clientPrefabs.Count)];
    }

    private ClientRequestData PickRandomRequest()
    {
        if (possibleRequests == null || possibleRequests.Count == 0)
            return null;

        return possibleRequests[UnityEngine.Random.Range(0, possibleRequests.Count)];
    }

    private void NotifyChanged()
    {
        OnAppointmentsChanged?.Invoke();
    }

    public void NotifyClientFinished(ClientNPC client)
    {
        if (client == null)
            return;

        for (int i = appointments.Count - 1; i >= 0; i--)
        {
            ClientAppointmentData appointment = appointments[i];

            if (appointment == null)
                continue;

            bool sameClient = appointment.spawnedClient == client;
            bool samePrefab = appointment.clientPrefab == client.SourcePrefab && appointment.spawned;

            if (sameClient || samePrefab)
            {
                appointment.status = ClientAppointmentStatus.Completed;
                appointments.RemoveAt(i);

                if (enableDebugLogs)
                    Debug.Log($"[Agenda] Agendamento removido após saída do cliente: {client.ClientDisplayName}");

                NotifyChanged();
                return;
            }
        }
    }

    [ContextMenu("TESTE - Gerar Agendamento Agora")]
    private void TestGenerateAppointmentNow()
    {
        TryGenerateAppointment();
    }
}