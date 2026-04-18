using UnityEngine;

[System.Serializable]
public class ServiceEvaluationResult
{
    [Range(0f, 5f)] public float waitingScore;
    [Range(0f, 5f)] public float serviceQualityScore;
    [Range(0f, 5f)] public float serviceTimeScore;
    [Range(0f, 5f)] public float barberConditionScore;
    [Range(0f, 5f)] public float equipmentScore;
    [Range(0f, 5f)] public float comfortScore;

    [Range(0f, 5f)] public float finalScore;

    public string waitingComment;
    public string qualityComment;
    public string timeComment;
    public string barberConditionComment;
    public string equipmentComment;
    public string comfortComment;
    public string finalComment;
}