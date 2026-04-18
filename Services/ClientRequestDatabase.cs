using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ClientRequestDatabase", menuName = "AfroBarber/Client/Request Database")]
public class ClientRequestDatabase : ScriptableObject
{
    [SerializeField] private List<ClientRequestData> requests = new List<ClientRequestData>();

    public List<ClientRequestData> GetAll()
    {
        return requests;
    }

    public ClientRequestData GetRandom()
    {
        if (requests == null || requests.Count == 0)
            return null;

        int index = Random.Range(0, requests.Count);
        return requests[index];
    }

    public ClientRequestData GetById(string requestId)
    {
        if (string.IsNullOrWhiteSpace(requestId))
            return null;

        return requests.Find(r =>
            r != null &&
            (
                r.requestId == requestId ||
                r.id == requestId
            )
        );
    }
}