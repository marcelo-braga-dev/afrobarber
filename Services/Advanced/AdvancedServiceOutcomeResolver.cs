using System.Collections.Generic;
using UnityEngine;

public static class AdvancedServiceOutcomeResolver
{
    public static AdvancedServiceResult Resolve(ClientNPC client, ClientRequestData request, ServicePlanData plan, List<ServiceActionExecutionResult> actionResults, float actualMinutes)
    {
        AdvancedServiceResult result = new AdvancedServiceResult();

        float expectedClientMinutes = request != null ? request.ServiceTime : 10f;
        float estimatedPlanMinutes = plan != null ? plan.GetEstimatedTotalMinutes() : expectedClientMinutes;

        float qualityAvg = 2.5f;
        if (actionResults != null && actionResults.Count > 0)
        {
            float sum = 0f;
            for (int i = 0; i < actionResults.Count; i++)
                sum += actionResults[i].score;

            qualityAvg = sum / actionResults.Count;
        }

        float timeDelta = Mathf.Abs(actualMinutes - expectedClientMinutes);
        float timePenalty = Mathf.Clamp01(timeDelta / Mathf.Max(2f, expectedClientMinutes));

        NPCIdentity identity = client != null ? client.GetComponent<NPCIdentity>() : null;
        float personalityFactor = 1f;
        if (identity != null && identity.Personality == NPCPersonality.Demanding)
            personalityFactor = 1.15f;

        float finalScore = Mathf.Clamp(qualityAvg - (timePenalty * 2f * personalityFactor), 0f, 5f);

        int baseMoney = request != null ? request.ServicePrice : 30;
        int baseXp = request != null ? request.XPReward : 10;

        float rewardMultiplier = Mathf.Lerp(0.55f, 1.4f, finalScore / 5f);

        result.expectedClientMinutes = expectedClientMinutes;
        result.estimatedPlanMinutes = estimatedPlanMinutes;
        result.actualTotalMinutes = actualMinutes;
        result.finalScore = finalScore;
        result.finalRating = ToFinalRating(finalScore);
        result.moneyReward = Mathf.RoundToInt(baseMoney * rewardMultiplier);
        result.xpReward = Mathf.RoundToInt(baseXp * rewardMultiplier);

        return result;
    }

    private static ServiceFinalRating ToFinalRating(float score)
    {
        if (score < 0.9f) return ServiceFinalRating.Horrivel;
        if (score < 1.8f) return ServiceFinalRating.Ruim;
        if (score < 2.8f) return ServiceFinalRating.MaisOuMenos;
        if (score < 3.8f) return ServiceFinalRating.Bom;
        if (score < 4.6f) return ServiceFinalRating.Maravilhoso;
        return ServiceFinalRating.Perfeito;
    }
}
