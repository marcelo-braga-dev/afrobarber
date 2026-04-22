using UnityEngine;

public class NPCConversationBrain : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private NPCIdentity identity;
    [SerializeField] private NPCSocialProfile socialProfile;
    [SerializeField] private NPCDialogueMemory dialogueMemory;
    [SerializeField] private NPCInteractionIndicator interactionIndicator;
    [SerializeField] private ClientNPC clientNPC;
    [SerializeField] private ClientPatience patience;

    [Header("Automação")]
    [SerializeField] private bool autoTalkEnabled = true;
    [SerializeField] private float minTalkIntervalSeconds = 15f;
    [SerializeField] private float maxTalkIntervalSeconds = 28f;

    private float nextTalkTime;

    public NPCIdentity Identity => identity;
    public NPCDialogueMemory Memory => dialogueMemory;

    private void Awake()
    {
        if (identity == null)
            identity = GetComponent<NPCIdentity>();

        if (socialProfile == null)
            socialProfile = GetComponent<NPCSocialProfile>();

        if (dialogueMemory == null)
            dialogueMemory = GetComponent<NPCDialogueMemory>();

        if (interactionIndicator == null)
            interactionIndicator = GetComponentInChildren<NPCInteractionIndicator>(true);

        if (clientNPC == null)
            clientNPC = GetComponent<ClientNPC>();

        if (patience == null)
            patience = GetComponent<ClientPatience>();

        ScheduleNextTalk();
    }

    private void Update()
    {
        if (!autoTalkEnabled || GlobalDialogueManager.Instance == null || identity == null)
            return;

        if (Time.time < nextTalkTime)
            return;

        EmitAutoDialogue();
        ScheduleNextTalk();
    }

    public void ProcessPlayerOption(DialogueSpeechOption option)
    {
        float patiencePercent = patience != null ? patience.PatiencePercent : 0.25f;
        DialogueReactionEvaluator.ApplyPlayerOption(identity, socialProfile, dialogueMemory, option, patiencePercent);

        DialogueSocialIntent responseIntent = SelectNpcResponseIntent(option);
        DialogueContextType context = option != null ? option.contextType : ResolveCurrentContext();
        TrySpeak(context, responseIntent);
    }

    public DialogueContextType ResolveCurrentContext()
    {
        bool inService = clientNPC != null && clientNPC.CurrentState == ClientNPC.ClientState.InService;
        bool inQueue = clientNPC != null && clientNPC.CurrentState == ClientNPC.ClientState.WaitingForService;
        return DialogueContextResolver.ResolveForNpc(clientNPC, inService, inQueue, false);
    }

    private void EmitAutoDialogue()
    {
        DialogueContextType context = ResolveCurrentContext();

        DialogueSocialIntent intent;
        float patiencePercent = patience != null ? patience.PatiencePercent : 0f;

        if (patiencePercent > 0.8f)
            intent = DialogueSocialIntent.Complain;
        else if (patiencePercent > 0.5f)
            intent = DialogueSocialIntent.AskAttention;
        else
            intent = DialogueSocialIntent.InterruptSilence;

        TrySpeak(context, intent);
    }

    private void TrySpeak(DialogueContextType context, DialogueSocialIntent intent)
    {
        DialogueLineEntry line = NPCDialogueRepertoire.PickLine(identity.NpcId, context, identity.Mood, intent, identity.RelationshipLevel);
        if (line == null || string.IsNullOrWhiteSpace(line.text))
            return;

        GlobalDialogueManager.Instance.AddNpcMessage(identity, line.text, context);
        dialogueMemory?.RegisterImmediateFact($"npc_{intent}", line.text);

        if (!string.IsNullOrWhiteSpace(line.unlockTopic))
            dialogueMemory?.AddTopic(line.unlockTopic);

        if (interactionIndicator != null)
            interactionIndicator.SetState(InteractionAvailabilityType.NeedResponse);
    }

    private DialogueSocialIntent SelectNpcResponseIntent(DialogueSpeechOption option)
    {
        if (option == null)
            return DialogueSocialIntent.AnswerPlayer;

        switch (option.intent)
        {
            case DialogueSocialIntent.Apologize:
            case DialogueSocialIntent.AskPatience:
            case DialogueSocialIntent.ReduceTension:
                return DialogueSocialIntent.ReduceTension;
            case DialogueSocialIntent.CommentCity:
            case DialogueSocialIntent.CommentCulture:
                return option.intent;
            case DialogueSocialIntent.Promise:
                return DialogueSocialIntent.ChargePromise;
            default:
                return DialogueSocialIntent.AnswerPlayer;
        }
    }

    private void ScheduleNextTalk()
    {
        nextTalkTime = Time.time + Random.Range(minTalkIntervalSeconds, maxTalkIntervalSeconds);
    }
}
