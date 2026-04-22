using System.Collections.Generic;

public static class DialogueOptionGenerator
{
    public static List<DialogueSpeechOption> BuildOptions(DialogueContextType context, NPCIdentity identity, NPCDialogueMemory memory)
    {
        List<DialogueSpeechOption> options = new List<DialogueSpeechOption>();

        float relationship = identity != null ? identity.RelationshipLevel : 0f;
        float tension = memory != null ? memory.Data.tension : 0.35f;

        if (context == DialogueContextType.Queue)
        {
            options.Add(Create("queue_call_next", "Próximo da fila", context, DialogueSpeechOptionAction.CallNextClient, DialogueSocialIntent.ProfessionalAssurance, DialogueOptionTone.Professional, -0.05f, 0.05f, 0f));
            options.Add(Create("queue_ask_patience", "Já vou te chamar, segura só um pouco", context, DialogueSpeechOptionAction.AskForPatience, DialogueSocialIntent.AskPatience, DialogueOptionTone.Empathetic, -0.15f, 0.05f, 0.2f));
            options.Add(Create("queue_apologize", "Foi mal pela demora, vou organizar aqui", context, DialogueSpeechOptionAction.Apologize, DialogueSocialIntent.Apologize, DialogueOptionTone.Empathetic, -0.2f, 0.08f, 0.15f));
            options.Add(Create("queue_close", "Hoje não vou atender mais ninguém", context, DialogueSpeechOptionAction.CloseShopForNewClients, DialogueSocialIntent.ProfessionalAssurance, DialogueOptionTone.Firm, 0.2f, -0.1f, -0.1f));
        }

        if (context == DialogueContextType.Service)
        {
            options.Add(Create("service_ask_pref", "Qual estilo você prefere?", context, DialogueSpeechOptionAction.AskServicePreference, DialogueSocialIntent.ProfessionalAssurance, DialogueOptionTone.Professional, -0.1f, 0.1f, 0.05f));
            options.Add(Create("service_promise_priority", "Vou priorizar seu acabamento", context, DialogueSpeechOptionAction.PromisePriority, DialogueSocialIntent.Promise, DialogueOptionTone.Professional, -0.1f, 0.12f, 0.05f));
            options.Add(Create("service_start", "Começar atendimento", context, DialogueSpeechOptionAction.StartService, DialogueSocialIntent.ProfessionalAssurance, DialogueOptionTone.Neutral, -0.05f, 0.05f, 0f));
        }

        if (context == DialogueContextType.City || context == DialogueContextType.CulturalEvent)
        {
            options.Add(Create("city_comment", "E esse movimento do bairro hoje?", context, DialogueSpeechOptionAction.CommentCity, DialogueSocialIntent.CommentCity, DialogueOptionTone.Playful, -0.05f, 0.04f, 0f));
            options.Add(Create("culture_comment", "Vai colar no evento cultural mais tarde?", context, DialogueSpeechOptionAction.CommentCulture, DialogueSocialIntent.CommentCulture, DialogueOptionTone.Playful, -0.06f, 0.06f, 0f));
        }

        if (tension > 0.6f)
            options.Add(Create("calm_down", "Relaxa, vou resolver contigo agora", context, DialogueSpeechOptionAction.SayWillTakeLong, DialogueSocialIntent.ReduceTension, DialogueOptionTone.Empathetic, -0.25f, 0.1f, 0.15f));

        if (relationship > 30f)
            options.Add(Create("friendship", "Tu já é de casa, parceiro.", context, DialogueSpeechOptionAction.None, DialogueSocialIntent.BuildFriendship, DialogueOptionTone.Playful, -0.1f, 0.08f, 0f, 20f));

        if (options.Count == 0)
            options.Add(Create("fallback", "Sem conversa disponível agora", DialogueContextType.None, DialogueSpeechOptionAction.None, DialogueSocialIntent.None, DialogueOptionTone.Neutral, 0f, 0f, 0f));

        return options;
    }

    private static DialogueSpeechOption Create(
        string id,
        string label,
        DialogueContextType context,
        DialogueSpeechOptionAction action,
        DialogueSocialIntent intent,
        DialogueOptionTone tone,
        float tensionDelta,
        float trustDelta,
        float patienceDelta,
        float minRelationship = -100f)
    {
        return new DialogueSpeechOption
        {
            optionId = id,
            label = label,
            contextType = context,
            action = action,
            intent = intent,
            tone = tone,
            tensionDelta = tensionDelta,
            trustDelta = trustDelta,
            patienceDelta = patienceDelta,
            minRelationshipLevel = minRelationship,
            unlocksFollowUp = trustDelta > 0.09f
        };
    }
}
