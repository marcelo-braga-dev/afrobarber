using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NPCMemoryEntry
{
    public string key;
    public string note;
    public long unixTimestamp;
    public float impact;
}

public class NPCRelationshipMemory : MonoBehaviour
{
    [SerializeField] private NPCIdentity identity;
    [SerializeField] private List<NPCMemoryEntry> memories = new List<NPCMemoryEntry>();

    public IReadOnlyList<NPCMemoryEntry> Memories => memories;

    private void Awake()
    {
        if (identity == null)
            identity = GetComponent<NPCIdentity>();
    }

    public void AddMemory(string key, string note, float relationshipImpact)
    {
        NPCMemoryEntry entry = new NPCMemoryEntry
        {
            key = key,
            note = note,
            impact = relationshipImpact,
            unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        memories.Add(entry);

        if (identity != null)
            identity.AddRelationship(relationshipImpact);
    }
}
