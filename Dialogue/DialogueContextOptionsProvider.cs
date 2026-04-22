using System.Collections.Generic;

public static class DialogueContextOptionsProvider
{
    public static List<DialogueSpeechOption> GetOptions(DialogueContextType contextType)
    {
        return GetOptions(contextType, null);
    }

    public static List<DialogueSpeechOption> GetOptions(DialogueContextType contextType, NPCConversationBrain brain)
    {
        NPCIdentity identity = brain != null ? brain.Identity : null;
        NPCDialogueMemory memory = brain != null ? brain.Memory : null;

        List<DialogueSpeechOption> options = DialogueOptionGenerator.BuildOptions(contextType, identity, memory);

        if (options.Count == 0)
        {
            options.Add(new DialogueSpeechOption
            {
                optionId = "none_no_action",
                label = "Nenhuma conversa disponível agora",
                contextType = DialogueContextType.None,
                action = DialogueSpeechOptionAction.None,
                intent = DialogueSocialIntent.None,
                tone = DialogueOptionTone.Neutral
            });
        }

        return options;
    }
}
