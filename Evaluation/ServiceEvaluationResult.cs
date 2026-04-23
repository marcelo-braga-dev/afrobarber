using UnityEngine;

[System.Serializable]
public class ServiceEvaluationResult
{
    [Header("Notas internas")]
    [Range(0f, 5f)] public float waitingScore;
    [Range(0f, 5f)] public float serviceQualityScore;
    [Range(0f, 5f)] public float serviceTimeScore;
    [Range(0f, 5f)] public float barberConditionScore;
    [Range(0f, 5f)] public float equipmentScore;
    [Range(0f, 5f)] public float comfortScore;
    [Range(0f, 5f)] public float aestheticScore;
    [Range(0f, 5f)] public float pricingScore;

    [Header("Notas agrupadas para UI")]
    [Range(0f, 5f)] public float attendanceScore;
    [Range(0f, 5f)] public float structureScore;
    [Range(0f, 5f)] public float experienceScore;

    [Header("Nota final")]
    [Range(0f, 5f)] public float finalScore;

    [Header("Comentários internos")]
    public string waitingComment;
    public string qualityComment;
    public string timeComment;
    public string barberConditionComment;
    public string equipmentComment;
    public string comfortComment;
    public string aestheticComment;
    public string pricingComment;

    [Header("Comentários agrupados")]
    public string attendanceComment;
    public string structureComment;
    public string experienceComment;
    public string finalComment;
}