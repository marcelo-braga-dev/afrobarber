using System;
using UnityEngine;

public enum NPCType
{
    Client,
    Resident,
    Artist,
    Merchant,
    Staff,
    Tutorial
}

public enum NPCPersonality
{
    Friendly,
    Irritated,
    Calm,
    Demanding,
    Communicative,
    Annoying,
    Shy,
    Playful
}

public enum NPCMood
{
    Neutral,
    Happy,
    Impatient,
    Angry,
    Excited,
    Worried
}

public enum InteractionAvailabilityType
{
    None,
    Conversation,
    Urgent,
    Tutorial,
    NeedResponse
}

public class NPCIdentity : MonoBehaviour
{
    [SerializeField] private string npcId;
    [SerializeField] private string displayName = "NPC";
    [SerializeField] private NPCType npcType = NPCType.Client;
    [SerializeField] private NPCPersonality personality = NPCPersonality.Calm;
    [SerializeField] private NPCMood mood = NPCMood.Neutral;
    [SerializeField, Range(-100f, 100f)] private float relationshipLevel;
    [SerializeField] private Sprite portrait;

    public string NpcId => npcId;
    public string DisplayName => displayName;
    public NPCType NpcType => npcType;
    public NPCPersonality Personality => personality;
    public NPCMood Mood => mood;
    public float RelationshipLevel => relationshipLevel;
    public Sprite Portrait => portrait;

    public void SetMood(NPCMood newMood) => mood = newMood;

    public void AddRelationship(float amount)
    {
        relationshipLevel = Mathf.Clamp(relationshipLevel + amount, -100f, 100f);
    }

    public void EnsureIdentity()
    {
        if (string.IsNullOrWhiteSpace(npcId))
            npcId = Guid.NewGuid().ToString("N");

        if (string.IsNullOrWhiteSpace(displayName))
            displayName = "NPC-" + npcId.Substring(0, Mathf.Min(6, npcId.Length));
    }

    private void Awake()
    {
        EnsureIdentity();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        EnsureIdentity();
    }
#endif
}
