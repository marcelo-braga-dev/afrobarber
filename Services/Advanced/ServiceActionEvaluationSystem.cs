using UnityEngine;

public static class ServiceActionEvaluationSystem
{
    public static ServiceActionExecutionResult EvaluateStep(ServiceActionPlanStep step, ClientNPC client, float actualMinutes)
    {
        ServiceActionExecutionResult result = new ServiceActionExecutionResult
        {
            step = step,
            actualMinutes = actualMinutes
        };

        float score = 3f;

        if (step != null)
        {
            float durationPenalty = Mathf.Max(0f, actualMinutes - step.estimatedMinutes) * 0.35f;
            score -= durationPenalty;

            if (!string.IsNullOrWhiteSpace(step.productUniqueId) && InventoryManager.Instance != null)
            {
                ProductInventoryState state = InventoryManager.Instance.GetItemByUniqueId(step.productUniqueId);
                if (state != null)
                {
                    ProductData data = InventoryManager.Instance.GetProductDataById(state.productId);
                    if (data != null)
                    {
                        float quality = (data.precisao + data.velocidade + data.durabilidade) / 300f;
                        score += Mathf.Lerp(-0.3f, 1.8f, quality);

                        bool compatible = ServiceToolCompatibility.IsCompatible(step.actionType, data.category);
                        if (!compatible)
                            score -= 1.5f;
                    }
                }
                else
                {
                    score -= 1f;
                }
            }
            else
            {
                score -= 0.5f;
            }
        }

        NPCIdentity identity = client != null ? client.GetComponent<NPCIdentity>() : null;
        if (identity != null)
        {
            if (identity.Personality == NPCPersonality.Demanding)
                score -= 0.35f;
            else if (identity.Personality == NPCPersonality.Friendly)
                score += 0.25f;

            if (identity.Mood == NPCMood.Angry)
                score -= 0.45f;
            else if (identity.Mood == NPCMood.Happy)
                score += 0.15f;
        }

        score = Mathf.Clamp(score, 0f, 5f);
        result.score = score;
        result.rating = ToRating(score);
        result.feedback = GetFeedback(result.rating);

        return result;
    }

    private static ServiceActionRating ToRating(float score)
    {
        if (score < 0.9f) return ServiceActionRating.Horrivel;
        if (score < 1.8f) return ServiceActionRating.Ruim;
        if (score < 2.8f) return ServiceActionRating.MaisOuMenos;
        if (score < 3.8f) return ServiceActionRating.Bom;
        if (score < 4.6f) return ServiceActionRating.Otimo;
        return ServiceActionRating.Perfeito;
    }

    private static string GetFeedback(ServiceActionRating rating)
    {
        switch (rating)
        {
            case ServiceActionRating.Horrivel:
                return "Execução muito ruim.";
            case ServiceActionRating.Ruim:
                return "Ação abaixo do esperado.";
            case ServiceActionRating.MaisOuMenos:
                return "Ação mediana.";
            case ServiceActionRating.Bom:
                return "Boa execução.";
            case ServiceActionRating.Otimo:
                return "Ótima execução.";
            default:
                return "Execução perfeita!";
        }
    }
}
