using System.Collections.Generic;
using UnityEngine;

public class DialogueGameplayActionRouter : MonoBehaviour
{
    [SerializeField] private bool allowNewClients = true;

    private void OnEnable()
    {
        if (GlobalDialogueManager.Instance != null)
            GlobalDialogueManager.Instance.OnPlayerOptionTriggered += HandleOption;
    }

    private void OnDisable()
    {
        if (GlobalDialogueManager.Instance != null)
            GlobalDialogueManager.Instance.OnPlayerOptionTriggered -= HandleOption;
    }

    private void HandleOption(DialogueSpeechOption option)
    {
        if (option == null)
            return;

        switch (option.action)
        {
            case DialogueSpeechOptionAction.CallNextClient:
                CallNextClientFromQueue();
                break;

            case DialogueSpeechOptionAction.CloseShopForNewClients:
                allowNewClients = false;
                GlobalDialogueManager.Instance?.AddSystemMessage("Novos atendimentos foram pausados.", DialogueContextType.Queue);
                break;

            case DialogueSpeechOptionAction.OpenShopForNewClients:
                allowNewClients = true;
                GlobalDialogueManager.Instance?.AddSystemMessage("Atendimentos reabertos.", DialogueContextType.Queue);
                break;
        }
    }

    private void CallNextClientFromQueue()
    {
        if (!allowNewClients)
        {
            GlobalDialogueManager.Instance?.AddSystemMessage("Você encerrou os novos atendimentos por enquanto.", DialogueContextType.Queue);
            return;
        }

        if (BarberQueueSystem.Instance == null)
        {
            GlobalDialogueManager.Instance?.AddSystemMessage("Fila indisponível no momento.", DialogueContextType.Queue);
            return;
        }

        List<ClientQueueData> ordered = BarberQueueSystem.Instance.GetQueueOrderedByArrival();
        ClientQueueData next = ordered.Find(x => x != null && !x.isBeingServed && x.clientNPC != null);

        if (next == null)
        {
            GlobalDialogueManager.Instance?.AddSystemMessage("Não há clientes aguardando.", DialogueContextType.Queue);
            return;
        }

        ClientNPC npc = next.clientNPC;
        NPCIdentity identity = npc.GetComponent<NPCIdentity>();
        GlobalDialogueManager.Instance?.AddNpcMessage(identity, "Tô indo pra cadeira agora!", DialogueContextType.Queue);

        npc.CallForService();
    }
}
