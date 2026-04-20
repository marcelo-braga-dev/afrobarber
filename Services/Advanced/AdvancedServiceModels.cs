using System;
using System.Collections.Generic;

public enum ServiceActionType
{
    Wash,
    Comb,
    Cut,
    Razor,
    Finish,
    Define,
    Beard,
    Finalize
}

public enum ServiceActionRating
{
    Horrivel,
    Ruim,
    MaisOuMenos,
    Bom,
    Otimo,
    Perfeito
}

public enum ServiceFinalRating
{
    Horrivel,
    Ruim,
    MaisOuMenos,
    Bom,
    Maravilhoso,
    Perfeito
}

[Serializable]
public class ServiceActionDefinition
{
    public ServiceActionType actionType;
    public float baseMinutes = 2f;
}

[Serializable]
public class ServiceActionPlanStep
{
    public int order;
    public ServiceActionType actionType;
    public string productUniqueId;
    public string productId;
    public float estimatedMinutes;
}

[Serializable]
public class ServicePlanData
{
    public string serviceId;
    public List<ServiceActionPlanStep> steps = new List<ServiceActionPlanStep>();

    public float GetEstimatedTotalMinutes()
    {
        float total = 0f;
        foreach (ServiceActionPlanStep step in steps)
            total += Math.Max(0.1f, step.estimatedMinutes);

        return total;
    }
}

[Serializable]
public class ServiceActionExecutionResult
{
    public ServiceActionPlanStep step;
    public float actualMinutes;
    public float score;
    public ServiceActionRating rating;
    public string feedback;
}

[Serializable]
public class AdvancedServiceResult
{
    public float expectedClientMinutes;
    public float estimatedPlanMinutes;
    public float actualTotalMinutes;
    public ServiceFinalRating finalRating;
    public float finalScore;
    public int moneyReward;
    public int xpReward;
}
