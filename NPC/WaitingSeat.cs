using UnityEngine;

public class WaitingSeat : MonoBehaviour
{
    [SerializeField] private Transform approachPoint;
    [SerializeField] private Transform sitPoint;

    private ClientNPC currentClient;

    public Transform ApproachPoint => approachPoint != null ? approachPoint : SitPoint;
    public Transform SitPoint => sitPoint != null ? sitPoint : transform;
    public bool IsOccupied => currentClient != null;

    public bool TryReserve(ClientNPC client)
    {
        if (IsOccupied) return false;

        currentClient = client;
        return true;
    }

    public void Release(ClientNPC client)
    {
        if (currentClient == client)
        {
            currentClient = null;
        }
    }
}