using UnityEngine;

[System.Serializable]
public class ClientQueueData
{
    public ClientNPC clientNPC;
    public string clientName;
    public float arrivalGameMinutes;
    public float maxPatienceMinutes;
    public bool isPriority;
    public bool isBeingServed;

    public ClientQueueData(ClientNPC clientNPC, string clientName, float arrivalGameMinutes, float maxPatienceMinutes)
    {
        this.clientNPC = clientNPC;
        this.clientName = clientName;
        this.arrivalGameMinutes = arrivalGameMinutes;
        this.maxPatienceMinutes = maxPatienceMinutes;
        this.isPriority = false;
        this.isBeingServed = false;
    }

    public float GetWaitingMinutes(float currentGameMinutes)
    {
        return Mathf.Max(0f, currentGameMinutes - arrivalGameMinutes);
    }

    public float GetPatiencePercent(float currentGameMinutes)
    {
        if (maxPatienceMinutes <= 0f)
            return 0f;

        float waiting = GetWaitingMinutes(currentGameMinutes);
        return Mathf.Clamp01(waiting / maxPatienceMinutes);
    }
}