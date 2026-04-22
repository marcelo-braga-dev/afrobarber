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
    CulturalEvent,
    Farewell,
    Complaint,
    Friendship
}

public enum DialogueSpeechOptionAction
{
    None,
    CallNextClient,
    CloseShopForNewClients,
    OpenShopForNewClients,
    AskServicePreference,
    StartService,
    SayWillTakeLong,
    AskForPatience,
    Apologize,
    PromisePriority,
    CommentCity,
    CommentCulture,
    EndConversation
}

public enum DialogueSocialIntent
{
    None,
    Greet,
    AskAttention,
    Complain,
    Praise,
    AskSpeed,
    AskPrecision,
    ReaffirmPreference,
    Joke,
    CommentCity,
    CommentCulture,
    ChargePromise,
    Farewell,
    BuildFriendship,
    ReduceTension,
    Pressure,
    InterruptSilence,
    Teach,
    AnswerPlayer,
    AskPatience,
    ProfessionalAssurance,
    Apologize,
    Promise
}

public enum DialogueOptionTone
{
    Neutral,
    Empathetic,
    Professional,
    Playful,
    Firm
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
    public DialogueSocialIntent intent;
    public DialogueOptionTone tone;
    [Range(-1f, 1f)] public float tensionDelta;
    [Range(-1f, 1f)] public float trustDelta;
    [Range(-1f, 1f)] public float patienceDelta;
    public float minRelationshipLevel;
    public bool unlocksFollowUp;
}

[Serializable]
public class DialogueConversation
{
    public string conversationId;
    public DialogueContextType contextType;
    public bool isGroup;
    public List<DialogueParticipant> participants = new List<DialogueParticipant>();
}