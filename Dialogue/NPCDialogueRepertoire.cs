using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueLineEntry
{
    public string id;
    public string npcId;
    public DialogueContextType context;
    public NPCMood mood;
    public DialogueSocialIntent intent;
    public string text;
    public float minRelationship;
    public string unlockTopic;
}

[Serializable]
public class DialogueCatalog
{
    public List<DialogueLineEntry> lines = new List<DialogueLineEntry>();
}

public static class NPCDialogueRepertoire
{
    private static DialogueCatalog catalog;

    public static DialogueLineEntry PickLine(
        string npcId,
        DialogueContextType context,
        NPCMood mood,
        DialogueSocialIntent intent,
        float relationship)
    {
        EnsureLoaded();

        if (catalog == null || catalog.lines == null || catalog.lines.Count == 0)
            return null;

        List<DialogueLineEntry> candidates = catalog.lines.FindAll(line =>
            (string.IsNullOrWhiteSpace(line.npcId) || string.Equals(line.npcId, npcId, StringComparison.OrdinalIgnoreCase)) &&
            line.context == context &&
            line.intent == intent &&
            line.minRelationship <= relationship &&
            (line.mood == mood || line.mood == NPCMood.Neutral));

        if (candidates.Count == 0)
        {
            candidates = catalog.lines.FindAll(line =>
                (string.IsNullOrWhiteSpace(line.npcId) || string.Equals(line.npcId, npcId, StringComparison.OrdinalIgnoreCase)) &&
                line.context == context &&
                line.intent == intent);
        }

        if (candidates.Count == 0)
            return null;

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    private static void EnsureLoaded()
    {
        if (catalog != null)
            return;

        TextAsset asset = Resources.Load<TextAsset>("Dialogue/dialogue_repertoire");
        if (asset == null)
        {
            catalog = new DialogueCatalog();
            return;
        }

        catalog = JsonUtility.FromJson<DialogueCatalog>(asset.text);
        if (catalog == null)
            catalog = new DialogueCatalog();
    }
}
