using System.Collections.Generic;

public static class DialogueContextOptionsProvider
{
    public static List<DialogueSpeechOption> GetOptions(DialogueContextType context)
    {
        List<DialogueSpeechOption> options = new List<DialogueSpeechOption>();

        switch (context)
        {
            case DialogueContextType.Queue:
                options.Add(Create("queue_next", "Próximo da fila", context, DialogueSpeechOptionAction.CallNextClient));
                options.Add(Create("queue_close", "Hoje não vou atender mais ninguém", context, DialogueSpeechOptionAction.CloseShopForNewClients));
                options.Add(Create("queue_wait", "Aguarde um momento", context, DialogueSpeechOptionAction.None));
                break;

            case DialogueContextType.Service:
                options.Add(Create("service_pref", "Como você quer o corte?", context, DialogueSpeechOptionAction.AskServicePreference));
                options.Add(Create("service_start", "Já estou começando", context, DialogueSpeechOptionAction.StartService));
                options.Add(Create("service_long", "Vai demorar um pouco", context, DialogueSpeechOptionAction.SayWillTakeLong));
                break;

            default:
                options.Add(Create("generic_hi", "Tudo bem por aí?", context, DialogueSpeechOptionAction.None));
                break;
        }

        return options;
    }

    private static DialogueSpeechOption Create(string id, string label, DialogueContextType context, DialogueSpeechOptionAction action)
    {
        return new DialogueSpeechOption
        {
            optionId = id,
            label = label,
            contextType = context,
            action = action
        };
    }
}
