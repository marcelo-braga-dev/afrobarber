using System;
using System.Collections.Generic;
using UnityEngine;

public enum ChatMessageType
{
    Player,
    Npc,
    System,
    Event,
    Group
}

public enum DialogueContextType
{
    None,
    Queue,
    Service,
    City,
    Tutorial,
    CulturalEvent
}

public enum DialogueSpeechOptionAction
{
    None,
    CallNextClient,
    CloseShopForNewClients,
    OpenShopForNewClients,
    AskServicePreference,
    StartService,
    SayWillTakeLong
}

[Serializable]
public class ChatMessageData
{
    public string senderId;
    public string senderDisplayName;
    [TextArea(2, 4)] public string text;
    public ChatMessageType messageType;
    public DialogueContextType contextType;
    public long unixTimestamp;

    public string GetHudFormattedText()
    {
        string sender = string.IsNullOrWhiteSpace(senderDisplayName) ? "Sistema" : senderDisplayName;
        return $"{sender}: {text}";
    }
}

[Serializable]
public class DialogueParticipant
{
    public string npcId;
    public string displayName;
    public Sprite portrait;
    public NPCPersonality personality;
    public NPCMood mood;
}

[Serializable]
public class DialogueSpeechOption
{
    public string optionId;
    public string label;
    public DialogueContextType contextType;
    public DialogueSpeechOptionAction action;
}

[Serializable]
public class DialogueConversation
{
    public string conversationId;
    public DialogueContextType contextType;
    public bool isGroup;
    public List<DialogueParticipant> participants = new List<DialogueParticipant>();
}
