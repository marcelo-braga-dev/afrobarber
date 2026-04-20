using UnityEngine;

public class DialogueGameplayActionRouter : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private BarbershopServiceManager serviceManager;

    private bool isSubscribed;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();

        if (serviceManager == null)
            serviceManager = FindObjectOfType<BarbershopServiceManager>();
    }

    private void OnDisable()
    {
        Unsubscribe();
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

        Debug.Log("[DialogueGameplayActionRouter] Inscrito no evento OnPlayerOptionTriggered.");
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
        {
            Debug.LogWarning("[DialogueGameplayActionRouter] Opção recebida está nula.");
            return;
        }

        Debug.Log($"[DialogueGameplayActionRouter] Recebeu opção: {option.label} | Action: {option.action}");

        switch (option.action)
        {
            case DialogueSpeechOptionAction.CallNextClient:
                CallNextClient();
                break;

            case DialogueSpeechOptionAction.CloseShopForNewClients:
                GlobalDialogueManager.Instance.AddSystemMessage(
                    "Você decidiu não receber mais novos clientes hoje.",
                    DialogueContextType.Queue
                );
                break;

            case DialogueSpeechOptionAction.OpenShopForNewClients:
                GlobalDialogueManager.Instance.AddSystemMessage(
                    "Você voltou a receber novos clientes.",
                    DialogueContextType.Queue
                );
                break;

            case DialogueSpeechOptionAction.AskServicePreference:
                GlobalDialogueManager.Instance.AddSystemMessage(
                    "Você perguntou ao cliente sobre a preferência do atendimento.",
                    option.contextType
                );
                break;

            case DialogueSpeechOptionAction.StartService:
                CallNextClient();
                break;

            case DialogueSpeechOptionAction.SayWillTakeLong:
                GlobalDialogueManager.Instance.AddSystemMessage(
                    "Você avisou que o atendimento pode demorar um pouco.",
                    option.contextType
                );
                break;

            case DialogueSpeechOptionAction.None:
            default:
                Debug.Log("[DialogueGameplayActionRouter] Nenhuma ação configurada para esta opção.");
                break;
        }
    }

    private void CallNextClient()
    {
        if (serviceManager == null)
            serviceManager = FindObjectOfType<BarbershopServiceManager>();

        if (serviceManager == null)
        {
            Debug.LogWarning("[DialogueGameplayActionRouter] BarbershopServiceManager não encontrado.");
            return;
        }

        bool success = serviceManager.CallNextClientFromQueue();

        if (success)
        {
            GlobalDialogueManager.Instance.AddSystemMessage(
                "Chamando o próximo cliente da fila.",
                DialogueContextType.Queue
            );
        }
        else
        {
            GlobalDialogueManager.Instance.AddSystemMessage(
                "Não há clientes disponíveis na fila.",
                DialogueContextType.Queue
            );
        }
    }
}