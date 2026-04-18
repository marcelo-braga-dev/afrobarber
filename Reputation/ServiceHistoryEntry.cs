using UnityEngine;

[System.Serializable]
public class ServiceHistoryEntry
{
    public int day;
    public float timeOfDayMinutes;
    public string clientName;
    public float receivedValue;
    public float finalRating;
    public string serviceName;

    public ServiceHistoryEntry(int day, float timeOfDayMinutes, string clientName, float receivedValue, float finalRating, string serviceName = "")
    {
        this.day = day;
        this.timeOfDayMinutes = timeOfDayMinutes;
        this.clientName = clientName;
        this.receivedValue = receivedValue;
        this.finalRating = finalRating;
        this.serviceName = serviceName;
    }
}