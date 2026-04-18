using UnityEngine;

[CreateAssetMenu(fileName = "ClientServiceProfile", menuName = "AfroBarber/Client/Service Profile")]
public class ClientServiceProfile : ScriptableObject
{
    [Header("Identificação")]
    public string profileId;
    public string profileName;

    [Header("Pedido fixo deste cliente")]
    public ClientRequestData requestData;

    [Header("Visual inicial/final")]
    public string beforeHairId;
    public string afterHairId;

    [Header("Apresentação")]
    [TextArea(2, 4)]
    public string notes;
}