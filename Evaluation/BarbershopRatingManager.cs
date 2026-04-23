using UnityEngine;
using UnityEngine.Events;

public class BarbershopRatingManager : MonoBehaviour
{
    public static BarbershopRatingManager Instance { get; private set; }

    [Header("Avaliação Global")]
    [SerializeField] private float globalRating = 3f;
    [SerializeField] private float attendanceRating = 3f;
    [SerializeField] private float structureRating = 3f;
    [SerializeField] private float experienceRating = 3f;
    [SerializeField] private int totalReviews;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    public float GlobalRating => globalRating;
    public float AttendanceRating => attendanceRating;
    public float StructureRating => structureRating;
    public float ExperienceRating => experienceRating;
    public int TotalReviews => totalReviews;

    public UnityEvent<float> OnRatingChanged;

    private const string RatingKey = "AFROBARBER_GLOBAL_RATING";
    private const string AttendanceKey = "AFROBARBER_ATTENDANCE_RATING";
    private const string StructureKey = "AFROBARBER_STRUCTURE_RATING";
    private const string ExperienceKey = "AFROBARBER_EXPERIENCE_RATING";
    private const string ReviewsKey = "AFROBARBER_TOTAL_REVIEWS";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();

        if (enableDebugLogs)
        {
            Debug.Log(
                $"[BarbershopRatingManager] Awake | " +
                $"Global={globalRating:0.0} | Atendimento={attendanceRating:0.0} | " +
                $"Estrutura={structureRating:0.0} | Experiência={experienceRating:0.0} | Reviews={totalReviews}"
            );
        }
    }

    public void AddReview(ServiceEvaluationResult result)
    {
        if (result == null)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[BarbershopRatingManager] AddReview(ServiceEvaluationResult): resultado nulo.");
            return;
        }

        AddReview(
            result.finalScore,
            result.attendanceScore,
            result.structureScore,
            result.experienceScore
        );
    }

    public void AddReview(float rating)
    {
        AddReview(rating, rating, rating, rating);
    }

    public void AddReview(float finalRating, float attendance, float structure, float experience)
    {
        finalRating = Mathf.Clamp(finalRating, 0f, 5f);
        attendance = Mathf.Clamp(attendance, 0f, 5f);
        structure = Mathf.Clamp(structure, 0f, 5f);
        experience = Mathf.Clamp(experience, 0f, 5f);

        float totalGlobalScore = globalRating * totalReviews;
        float totalAttendanceScore = attendanceRating * totalReviews;
        float totalStructureScore = structureRating * totalReviews;
        float totalExperienceScore = experienceRating * totalReviews;

        totalReviews++;

        globalRating = (totalGlobalScore + finalRating) / totalReviews;
        attendanceRating = (totalAttendanceScore + attendance) / totalReviews;
        structureRating = (totalStructureScore + structure) / totalReviews;
        experienceRating = (totalExperienceScore + experience) / totalReviews;

        Save();

        if (enableDebugLogs)
        {
            Debug.Log(
                $"[BarbershopRatingManager] Nova avaliação registrada | " +
                $"Final={finalRating:0.0} | Atendimento={attendance:0.0} | " +
                $"Estrutura={structure:0.0} | Experiência={experience:0.0} | " +
                $"Médias => Global={globalRating:0.0}, Atendimento={attendanceRating:0.0}, " +
                $"Estrutura={structureRating:0.0}, Experiência={experienceRating:0.0}, Reviews={totalReviews}"
            );
        }

        OnRatingChanged?.Invoke(globalRating);
    }

    public void ResetRatings()
    {
        globalRating = 3f;
        attendanceRating = 3f;
        structureRating = 3f;
        experienceRating = 3f;
        totalReviews = 0;

        Save();

        if (enableDebugLogs)
            Debug.Log("[BarbershopRatingManager] Avaliações resetadas.");

        OnRatingChanged?.Invoke(globalRating);
    }

    [ContextMenu("Teste Reputação")]
    private void TesteReputacao()
    {
        AddReview(4.2f, 4.6f, 3.4f, 4.0f);
    }

    [ContextMenu("Resetar Reputação Salva")]
    private void ResetarReputacaoSalva()
    {
        PlayerPrefs.DeleteKey(RatingKey);
        PlayerPrefs.DeleteKey(AttendanceKey);
        PlayerPrefs.DeleteKey(StructureKey);
        PlayerPrefs.DeleteKey(ExperienceKey);
        PlayerPrefs.DeleteKey(ReviewsKey);
        PlayerPrefs.Save();

        globalRating = 3f;
        attendanceRating = 3f;
        structureRating = 3f;
        experienceRating = 3f;
        totalReviews = 0;

        if (enableDebugLogs)
            Debug.Log("[BarbershopRatingManager] PlayerPrefs da reputação removidos.");

        OnRatingChanged?.Invoke(globalRating);
    }

    private void Save()
    {
        PlayerPrefs.SetFloat(RatingKey, globalRating);
        PlayerPrefs.SetFloat(AttendanceKey, attendanceRating);
        PlayerPrefs.SetFloat(StructureKey, structureRating);
        PlayerPrefs.SetFloat(ExperienceKey, experienceRating);
        PlayerPrefs.SetInt(ReviewsKey, totalReviews);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        globalRating = PlayerPrefs.GetFloat(RatingKey, 3f);
        attendanceRating = PlayerPrefs.GetFloat(AttendanceKey, 3f);
        structureRating = PlayerPrefs.GetFloat(StructureKey, 3f);
        experienceRating = PlayerPrefs.GetFloat(ExperienceKey, 3f);
        totalReviews = PlayerPrefs.GetInt(ReviewsKey, 0);
    }
}