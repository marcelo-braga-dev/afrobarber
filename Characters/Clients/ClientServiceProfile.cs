using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ClientServiceProfile", menuName = "AfroBarber/Client/Service Profile")]
public class ClientServiceProfile : ScriptableObject
{
    [Header("Identificação")]
    public string profileId;
    public string profileName;

    [Header("Pedido fixo deste cliente")]
    public ClientRequestData requestData;

    [Header("Pedidos possíveis deste cliente")]
    public List<ClientRequestData> possibleRequests = new List<ClientRequestData>();

    [Header("Configuração de sorteio")]
    public bool useRandomRequestFromList = false;

    [Header("Visual inicial/final padrão")]
    public string beforeHairId;
    public string afterHairId;

    [Header("Apresentação")]
    [TextArea(2, 4)]
    public string notes;

    public ClientRequestData GetRequest()
    {
        if (useRandomRequestFromList && possibleRequests != null && possibleRequests.Count > 0)
        {
            List<ClientRequestData> validRequests = new List<ClientRequestData>();

            foreach (ClientRequestData request in possibleRequests)
            {
                if (request != null)
                    validRequests.Add(request);
            }

            if (validRequests.Count > 0)
            {
                int index = Random.Range(0, validRequests.Count);
                return validRequests[index];
            }
        }

        return requestData;
    }
}