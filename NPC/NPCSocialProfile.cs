using UnityEngine;

public class NPCSocialProfile : MonoBehaviour
{
    [Header("Traços Profundos")]
    [SerializeField, Range(0f, 1f)] private float patience = 0.5f;
    [SerializeField, Range(0f, 1f)] private float sociability = 0.5f;
    [SerializeField, Range(0f, 1f)] private float demandLevel = 0.5f;
    [SerializeField, Range(0f, 1f)] private float qualitySensitivity = 0.5f;
    [SerializeField, Range(0f, 1f)] private float timeSensitivity = 0.5f;
    [SerializeField, Range(0f, 1f)] private float improvisationTolerance = 0.5f;
    [SerializeField, Range(0f, 1f)] private float resentment = 0.2f;
    [SerializeField, Range(0f, 1f)] private float praiseTendency = 0.5f;
    [SerializeField, Range(0f, 1f)] private float culturalInterest = 0.4f;
    [SerializeField, Range(0f, 1f)] private float intimacyOpenness = 0.3f;
    [SerializeField, Range(0f, 1f)] private float emotionalReactivity = 0.5f;

    [Header("Humor")]
    [SerializeField] private NPCMood baseMood = NPCMood.Neutral;

    public float Patience => patience;
    public float Sociability => sociability;
    public float DemandLevel => demandLevel;
    public float QualitySensitivity => qualitySensitivity;
    public float TimeSensitivity => timeSensitivity;
    public float ImprovisationTolerance => improvisationTolerance;
    public float Resentment => resentment;
    public float PraiseTendency => praiseTendency;
    public float CulturalInterest => culturalInterest;
    public float IntimacyOpenness => intimacyOpenness;
    public float EmotionalReactivity => emotionalReactivity;
    public NPCMood BaseMood => baseMood;

    public NPCMood EvaluateMood(float patiencePercent, float trust, float tension)
    {
        float moodScore = (trust * 0.45f) - (tension * 0.35f) - (patiencePercent * 0.3f * (1f + TimeSensitivity));
        moodScore += (0.5f - EmotionalReactivity) * 0.15f;

        if (moodScore > 0.55f)
            return NPCMood.Excited;

        if (moodScore > 0.20f)
            return NPCMood.Happy;

        if (moodScore > -0.15f)
            return baseMood;

        if (moodScore > -0.45f)
            return NPCMood.Impatient;

        return NPCMood.Angry;
    }
}
