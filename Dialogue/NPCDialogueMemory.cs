using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NPCDialogueSessionMemory
{
    public string key;
    public string value;
    public long unixTimestamp;
}

[Serializable]
public class NPCDialoguePersistentMemory
{
    public string npcId;
    public List<string> unlockedTopics = new List<string>();
    public List<string> recurringNotes = new List<string>();
    public List<NPCDialogueSessionMemory> lastSession = new List<NPCDialogueSessionMemory>();
    public float trust;
    public float tension;
}

public class NPCDialogueMemory : MonoBehaviour
{
    [SerializeField] private NPCIdentity identity;
    [SerializeField] private NPCRelationshipMemory relationshipMemory;
    [SerializeField] private NPCDialoguePersistentMemory data = new NPCDialoguePersistentMemory();

    public NPCDialoguePersistentMemory Data => data;

    private string SaveKey => $"npc_dialogue_memory_{data.npcId}";

    private void Awake()
    {
        if (identity == null)
            identity = GetComponent<NPCIdentity>();

        if (relationshipMemory == null)
            relationshipMemory = GetComponent<NPCRelationshipMemory>();

        if (identity != null)
            data.npcId = identity.NpcId;

        Load();
    }

    public void RegisterImmediateFact(string key, string value)
    {
        data.lastSession.Add(new NPCDialogueSessionMemory
        {
            key = key,
            value = value,
            unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        });

        TrimSession();
        Save();
    }

    public void RegisterRelationshipMemory(string key, string note, float impact)
    {
        relationshipMemory?.AddMemory(key, note, impact);
        RegisterImmediateFact(key, note);
    }

    public void AddTopic(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic) || data.unlockedTopics.Contains(topic))
            return;

        data.unlockedTopics.Add(topic);
        Save();
    }

    public void AddRecurringNote(string note)
    {
        if (string.IsNullOrWhiteSpace(note))
            return;

        data.recurringNotes.Add(note);

        if (data.recurringNotes.Count > 40)
            data.recurringNotes.RemoveRange(0, data.recurringNotes.Count - 40);

        Save();
    }

    public void AdjustSocialState(float trustDelta, float tensionDelta)
    {
        data.trust = Mathf.Clamp01(data.trust + trustDelta);
        data.tension = Mathf.Clamp01(data.tension + tensionDelta);
        Save();
    }

    public void Save()
    {
        if (string.IsNullOrWhiteSpace(data.npcId))
            return;

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
    }

    public void Load()
    {
        if (string.IsNullOrWhiteSpace(data.npcId))
            return;

        if (!PlayerPrefs.HasKey(SaveKey))
            return;

        string json = PlayerPrefs.GetString(SaveKey);
        NPCDialoguePersistentMemory loaded = JsonUtility.FromJson<NPCDialoguePersistentMemory>(json);

        if (loaded != null)
            data = loaded;
    }

    private void TrimSession()
    {
        if (data.lastSession.Count > 25)
            data.lastSession.RemoveRange(0, data.lastSession.Count - 25);
    }
}
