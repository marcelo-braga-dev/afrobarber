using UnityEngine;

public static class DialogueReactionEvaluator
{
    public static void ApplyPlayerOption(NPCIdentity identity, NPCSocialProfile profile, NPCDialogueMemory memory, DialogueSpeechOption option, float queuePatiencePercent)
    {
        if (option == null || memory == null)
            return;

        float trustScale = 0.8f + (profile != null ? profile.Sociability * 0.5f : 0.2f);
        float tensionScale = 0.8f + (profile != null ? profile.EmotionalReactivity * 0.5f : 0.2f);

        float trustDelta = option.trustDelta * trustScale;
        float tensionDelta = -option.tensionDelta * tensionScale;

        if (queuePatiencePercent > 0.75f && option.intent == DialogueSocialIntent.AskPatience)
            tensionDelta += 0.12f;

        memory.AdjustSocialState(trustDelta, tensionDelta);
        memory.RegisterImmediateFact($"option_{option.optionId}", option.label);

        if (option.unlocksFollowUp)
            memory.AddTopic($"followup_{option.intent}");

        NPCMood newMood = profile != null
            ? profile.EvaluateMood(queuePatiencePercent, memory.Data.trust, memory.Data.tension)
            : NPCMood.Neutral;

        identity?.SetMood(newMood);

        float relationshipImpact = Mathf.Clamp((trustDelta - tensionDelta) * 12f, -3f, 4f);
        memory.RegisterRelationshipMemory($"react_{option.optionId}", $"Reação à opção: {option.label}", relationshipImpact);
    }
}
