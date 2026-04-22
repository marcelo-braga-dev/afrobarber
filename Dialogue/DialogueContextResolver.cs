public static class DialogueContextResolver
{
    public static DialogueContextType ResolveForNpc(ClientNPC client, bool isInService, bool inQueue, bool isCulturalEventActive)
    {
        if (client == null)
            return DialogueContextType.None;

        if (isInService)
            return DialogueContextType.Service;

        if (inQueue)
            return DialogueContextType.Queue;

        if (isCulturalEventActive)
            return DialogueContextType.CulturalEvent;

        if (client.ServiceCompleted)
            return DialogueContextType.Farewell;

        return DialogueContextType.City;
    }
}
