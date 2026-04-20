using System;
using System.Collections.Generic;
using UnityEngine;

public class GlobalDialogueManager : MonoBehaviour
{
    public static GlobalDialogueManager Instance { get; private set; }

    [SerializeField] private bool logMessagesInConsole = true;
    [SerializeField] private int maxStoredMessages = 300;

    private readonly List<ChatMessageData> history = new List<ChatMessageData>();

    public IReadOnlyList<ChatMessageData> History => history;

    public event Action<ChatMessageData> OnMessageAdded;
    public event Action<DialogueConversation> OnConversationStarted;
    public event Action<DialogueSpeechOption> OnPlayerOptionTriggered;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public DialogueConversation StartConversation(DialogueContextType contextType, List<DialogueParticipant> participants)
    {
        DialogueConversation conversation = new DialogueConversation
        {
            conversationId = Guid.NewGuid().ToString("N"),
            contextType = contextType,
            isGroup = participants != null && participants.Count > 1,
            participants = participants ?? new List<DialogueParticipant>()
        };

        OnConversationStarted?.Invoke(conversation);
        return conversation;
    }

    public void AddSystemMessage(string text, DialogueContextType contextType = DialogueContextType.None)
    {
        AddMessage(CreateMessage("system", "Sistema", text, ChatMessageType.System, contextType));
    }

    public void AddPlayerMessage(string text, DialogueContextType contextType = DialogueContextType.None)
    {
        AddMessage(CreateMessage("player", "Você", text, ChatMessageType.Player, contextType));
    }

    public void AddNpcMessage(NPCIdentity identity, string text, DialogueContextType contextType = DialogueContextType.None, bool isGroup = false)
    {
        string senderId = identity != null ? identity.NpcId : "npc";
        string senderName = identity != null ? identity.DisplayName : "NPC";
        ChatMessageType type = isGroup ? ChatMessageType.Group : ChatMessageType.Npc;
        AddMessage(CreateMessage(senderId, senderName, text, type, contextType));
    }

    public void TriggerPlayerOption(DialogueSpeechOption option)
    {
        if (option == null)
            return;

        AddPlayerMessage(option.label, option.contextType);
        OnPlayerOptionTriggered?.Invoke(option);
    }

    private ChatMessageData CreateMessage(string senderId, string senderName, string text, ChatMessageType type, DialogueContextType contextType)
    {
        return new ChatMessageData
        {
            senderId = senderId,
            senderDisplayName = senderName,
            text = text,
            messageType = type,
            contextType = contextType,
            unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    private void AddMessage(ChatMessageData message)
    {
        if (message == null || string.IsNullOrWhiteSpace(message.text))
            return;

        history.Add(message);

        if (history.Count > maxStoredMessages)
            history.RemoveRange(0, history.Count - maxStoredMessages);

        if (logMessagesInConsole)
            Debug.Log($"[GlobalDialogue] {message.GetHudFormattedText()}");

        OnMessageAdded?.Invoke(message);
    }
}
