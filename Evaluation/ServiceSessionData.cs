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

    [Range(0f, 5f)] public float equipmentQualityScore = 3f;
    [Range(0f, 5f)] public float productQualityScore = 3f;

    public float barberEnergyAtStart;
    public float barberEnergyAtEnd;

    [Range(0f, 5f)] public float manualServiceQualityScore = 3f;

    [Header("Ambiente")]
    [Range(0f, 5f)] public float environmentComfortScore = 3f;
    [Range(0f, 5f)] public float environmentAestheticScore = 3f;

    public bool clientWasServed;
    public bool usedGoodProducts;
    public bool hadMistakes;

    public int finalChargedPrice;
    public int suggestedPrice;

    [Tooltip("0 = muito insatisfeito com o preço | 1 = totalmente satisfeito com o preço")]
    [Range(0f, 1f)]
    public float pricingSatisfactionScore = 1f;

    public float overworkMultiplier = 1f;

    public float GetAverageToolQuality()
    {
        return Mathf.Clamp((equipmentQualityScore + productQualityScore) * 0.5f, 0f, 5f);
    }
}