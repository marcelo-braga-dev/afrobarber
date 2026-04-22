using UnityEngine;

public class DialogueGameplayActionRouter : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private BarbershopServiceManager serviceManager;
    [SerializeField] private NPCConversationBrain focusedConversationBrain;

    private bool isSubscribed;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();

        if (serviceManager == null)
            serviceManager = FindFirstObjectByType<BarbershopServiceManager>();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void SetFocusedConversationBrain(NPCConversationBrain brain)
    {
        focusedConversationBrain = brain;
    }

    private void TrySubscribe()
    {
        if (isSubscribed)
            return;

        if (GlobalDialogueManager.Instance == null)
        {
            Debug.LogWarning("[DialogueGameplayActionRouter] GlobalDialogueManager ainda não existe. Tentando novamente...");
            Invoke(nameof(TrySubscribe), 0.2f);
            return;
        }

        GlobalDialogueManager.Instance.OnPlayerOptionTriggered -= HandlePlayerOptionTriggered;
        GlobalDialogueManager.Instance.OnPlayerOptionTriggered += HandlePlayerOptionTriggered;

        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        CancelInvoke(nameof(TrySubscribe));

        if (GlobalDialogueManager.Instance != null)
            GlobalDialogueManager.Instance.OnPlayerOptionTriggered -= HandlePlayerOptionTriggered;

        isSubscribed = false;
    }

    private void HandlePlayerOptionTriggered(DialogueSpeechOption option)
    {
        if (option == null)
            return;

        switch (option.action)
        {
            case DialogueSpeechOptionAction.CallNextClient:
                CallNextClient();
                break;

            case DialogueSpeechOptionAction.CloseShopForNewClients:
                GlobalDialogueManager.Instance.AddSystemMessage("Você decidiu não receber mais novos clientes hoje.", DialogueContextType.Queue);
                break;

            case DialogueSpeechOptionAction.OpenShopForNewClients:
                GlobalDialogueManager.Instance.AddSystemMessage("Você voltou a receber novos clientes.", DialogueContextType.Queue);
                break;

            case DialogueSpeechOptionAction.AskServicePreference:
                GlobalDialogueManager.Instance.AddSystemMessage("Você perguntou ao cliente sobre a preferência do atendimento.", option.contextType);
                break;

            case DialogueSpeechOptionAction.StartService:
                CallNextClient();
                break;

            case DialogueSpeechOptionAction.SayWillTakeLong:
                GlobalDialogueManager.Instance.AddSystemMessage("Você avisou que o atendimento pode demorar um pouco.", option.contextType);
                break;

            case DialogueSpeechOptionAction.AskForPatience:
                GlobalDialogueManager.Instance.AddSystemMessage("Você pediu calma e informou que já vai chamar.", option.contextType);
                break;

            case DialogueSpeechOptionAction.Apologize:
                GlobalDialogueManager.Instance.AddSystemMessage("Você pediu desculpas pela espera.", option.contextType);
                break;

            case DialogueSpeechOptionAction.PromisePriority:
                GlobalDialogueManager.Instance.AddSystemMessage("Você prometeu priorizar o acabamento desse cliente.", option.contextType);
                break;

            case DialogueSpeechOptionAction.CommentCity:
                GlobalDialogueManager.Instance.AddSystemMessage("Você comentou sobre o movimento da cidade.", option.contextType);
                break;

            case DialogueSpeechOptionAction.CommentCulture:
                GlobalDialogueManager.Instance.AddSystemMessage("Você comentou sobre um evento cultural local.", option.contextType);
                break;

            case DialogueSpeechOptionAction.EndConversation:
                GlobalDialogueManager.Instance.AddSystemMessage("Conversa encerrada.", option.contextType);
                break;
        }

        NPCConversationBrain brain = focusedConversationBrain != null
            ? focusedConversationBrain
            : FindFirstObjectByType<NPCConversationBrain>();
        brain?.ProcessPlayerOption(option);
    }

    private void CallNextClient()
    {
        if (serviceManager == null)
            serviceManager = FindFirstObjectByType<BarbershopServiceManager>();

        if (serviceManager == null)
            return;

        bool success = serviceManager.CallNextClientFromQueue();

        if (success)
            GlobalDialogueManager.Instance.AddSystemMessage("Chamando o próximo cliente da fila.", DialogueContextType.Queue);
        else
            GlobalDialogueManager.Instance.AddSystemMessage("Não há clientes disponíveis na fila.", DialogueContextType.Queue);
    }
}