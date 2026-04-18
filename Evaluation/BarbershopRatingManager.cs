using UnityEngine;
using UnityEngine.Events;

public class BarbershopRatingManager : MonoBehaviour
{
    public static BarbershopRatingManager Instance { get; private set; }

    [Header("Avaliação Global")]
    [SerializeField] private float globalRating = 3f;
    [SerializeField] private int totalReviews;

    public float GlobalRating => globalRating;
    public int TotalReviews => totalReviews;

    public UnityEvent<float> OnRatingChanged;

    private const string RatingKey = "AFROBARBER_GLOBAL_RATING";
    private const string ReviewsKey = "AFROBARBER_TOTAL_REVIEWS";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Load();
    }

    public void AddReview(float rating)
    {
        rating = Mathf.Clamp(rating, 0f, 5f);

        float totalScore = globalRating * totalReviews;
        totalReviews++;
        globalRating = (totalScore + rating) / totalReviews;

        Save();

        OnRatingChanged?.Invoke(globalRating);

        Debug.Log($"[BarbershopRatingManager] Nova avaliação: {rating:0.0}. Média global: {globalRating:0.0}");
    }

    private void Save()
    {
        PlayerPrefs.SetFloat(RatingKey, globalRating);
        PlayerPrefs.SetInt(ReviewsKey, totalReviews);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        globalRating = PlayerPrefs.GetFloat(RatingKey, 3f);
        totalReviews = PlayerPrefs.GetInt(ReviewsKey, 0);
    }
}