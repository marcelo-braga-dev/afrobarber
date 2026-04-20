using System.Collections.Generic;

public static class DialogueContextOptionsProvider
{
    public static List<DialogueSpeechOption> GetOptions(DialogueContextType contextType)
    {
        List<DialogueSpeechOption> options = new List<DialogueSpeechOption>();

        switch (contextType)
        {
            case DialogueContextType.Queue:
                options.Add(new DialogueSpeechOption
                {
                    optionId = "queue_call_next_client",
                    label = "Próximo da fila",
                    contextType = DialogueContextType.Queue,
                    action = DialogueSpeechOptionAction.CallNextClient
                });

                options.Add(new DialogueSpeechOption
                {
                    optionId = "queue_close_shop_new_clients",
                    label = "Hoje não vou atender mais ninguém",
                    contextType = DialogueContextType.Queue,
                    action = DialogueSpeechOptionAction.CloseShopForNewClients
                });

                options.Add(new DialogueSpeechOption
                {
                    optionId = "queue_open_shop_new_clients",
                    label = "Voltar a atender clientes",
                    contextType = DialogueContextType.Queue,
                    action = DialogueSpeechOptionAction.OpenShopForNewClients
                });
                break;

            case DialogueContextType.Service:
                options.Add(new DialogueSpeechOption
                {
                    optionId = "service_ask_preference",
                    label = "Qual estilo você prefere?",
                    contextType = DialogueContextType.Service,
                    action = DialogueSpeechOptionAction.AskServicePreference
                });

                options.Add(new DialogueSpeechOption
                {
                    optionId = "service_start",
                    label = "Começar atendimento",
                    contextType = DialogueContextType.Service,
                    action = DialogueSpeechOptionAction.StartService
                });
                break;

            case DialogueContextType.None:
            default:
                options.Add(new DialogueSpeechOption
                {
                    optionId = "none_no_action",
                    label = "Nenhuma conversa disponível agora",
                    contextType = DialogueContextType.None,
                    action = DialogueSpeechOptionAction.None
                });
                break;
        }

        return options;
    }
}