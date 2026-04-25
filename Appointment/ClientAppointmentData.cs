using System;
using UnityEngine;

public enum ClientAppointmentStatus
{
    Scheduled,
    WaitingToSpawn,
    Spawned,
    DelayedByQueue,
    Overbooked,
    Cancelled,
    Missed,
    Completed
}

[Serializable]
public class ClientAppointmentData
{
    public string id;
    public GameObject clientPrefab;
    public ClientRequestData requestData;

    [Header("Horário")]
    public int dayIndex;

    [Range(0, 23)]
    public int hour;

    [Range(0, 59)]
    public int minute;

    [Header("Estado")]
    public ClientAppointmentStatus status = ClientAppointmentStatus.Scheduled;
    public bool spawned;
    public bool cancelled;

    [Header("Atrasos")]
    public float cascadeDelayMinutes;
    public bool warningMessageSent;

    [NonSerialized] public ClientNPC spawnedClient;

    public float AppointmentTotalMinutes => dayIndex * 1440f + hour * 60f + minute;

    public string TimeText => $"{hour:00}:{minute:00}";
}