using UnityEngine;

[System.Serializable]
public class ServiceSessionData
{
    public ClientNPC clientNPC;
    public string clientName;

    public float serviceStartGameMinutes;
    public float serviceEndGameMinutes;

    public float clientArrivalGameMinutes;
    public float waitingMinutes;

    public float expectedServiceDurationMinutes;
    public float actualServiceDurationMinutes;
    public float maxAcceptableServiceDurationMinutes;

    public float equipmentQualityScore;
    public float productQualityScore;
    public float barberEnergyAtStart;
    public float barberEnergyAtEnd;

    public float manualServiceQualityScore;
    public float environmentComfortScore;

    public bool clientWasServed;
    public bool usedGoodProducts;
    public bool hadMistakes;

    public int finalChargedPrice;
    public int suggestedPrice;
    public float pricingSatisfactionScore = 1f;
    public float overworkMultiplier = 1f;

    public float GetAverageToolQuality()
    {
        return (equipmentQualityScore + productQualityScore) * 0.5f;
    }
}