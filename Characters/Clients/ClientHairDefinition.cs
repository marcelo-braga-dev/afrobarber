using UnityEngine;

[CreateAssetMenu(fileName = "ClientHairDefinition", menuName = "AfroBarber/Client/Hair Definition")]
public class ClientHairDefinition : ScriptableObject
{
    public string hairId;
    public string displayName;

    [Header("Opcional")]
    public Sprite icon;

    [TextArea(2, 4)]
    public string description;
}