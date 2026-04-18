using System;
using UnityEngine;

public class GlobalReputationSystem : MonoBehaviour
{
    public static GlobalReputationSystem Instance { get; private set; }

    [Header("Configuração")]
    [SerializeField] private bool usePlayerPrefs = true;
    [SerializeField] private string reputationSumKey = "AFROBARBER_REPUTATION_SUM";
    [SerializeField] private string totalRatingsKey = "AFROBARBER_TOTAL_RATINGS";

    [Header("Leitura")]
    [SerializeField] private float currentReputation = 0f;
    [SerializeField] private float ratingSum = 0f;
    [SerializeField] private int totalRatings = 0;

    public float Reputation => currentReputation;
    public float RatingSum => ratingSum;
    public int TotalRatings => totalRatings;

    public event Action<float> OnReputationChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadReputation();
        RecalculateReputation();
    }

    private void Start()
    {
        NotifyReputationChanged();
    }

    public void AddRating(float rating)
    {
        rating = Mathf.Clamp(rating, 0f, 5f);
        rating = Mathf.Round(rating * 10f) / 10f;

        ratingSum += rating;
        totalRatings++;

        RecalculateReputation();
        SaveReputation();

        Debug.Log($"[GlobalReputationSystem] AddRating chamado com nota {rating}");
        Debug.Log($"[GlobalReputationSystem] Soma: {ratingSum} | Total: {totalRatings} | Reputação atual: {currentReputation}");

        NotifyReputationChanged();
    }

    public void RemoveRating(float rating)
    {
        if (totalRatings <= 0)
            return;

        rating = Mathf.Clamp(rating, 0f, 5f);
        rating = Mathf.Round(rating * 10f) / 10f;

        ratingSum -= rating;
        totalRatings--;

        if (ratingSum < 0f)
            ratingSum = 0f;

        if (totalRatings < 0)
            totalRatings = 0;

        RecalculateReputation();
        SaveReputation();
        NotifyReputationChanged();
    }

    public void RebuildFromHistory(ServiceHistorySystem historySystem)
    {
        ratingSum = 0f;
        totalRatings = 0;

        if (historySystem != null)
        {
            var entries = historySystem.Entries;

            for (int i = 0; i < entries.Count; i++)
            {
                ratingSum += Mathf.Clamp(entries[i].finalRating, 0f, 5f);
                totalRatings++;
            }
        }

        RecalculateReputation();
        SaveReputation();
        NotifyReputationChanged();
    }

    public void ResetReputation()
    {
        currentReputation = 0f;
        ratingSum = 0f;
        totalRatings = 0;

        SaveReputation();
        NotifyReputationChanged();
    }

    private void RecalculateReputation()
    {
        if (totalRatings <= 0)
        {
            currentReputation = 0f;
            return;
        }

        currentReputation = ratingSum / totalRatings;
        currentReputation = Mathf.Round(currentReputation * 10f) / 10f;
        currentReputation = Mathf.Clamp(currentReputation, 0f, 5f);
    }

    private void SaveReputation()
    {
        if (!usePlayerPrefs)
            return;

        PlayerPrefs.SetFloat(reputationSumKey, ratingSum);
        PlayerPrefs.SetInt(totalRatingsKey, totalRatings);
        PlayerPrefs.Save();
    }

    private void LoadReputation()
    {
        if (!usePlayerPrefs)
            return;

        ratingSum = PlayerPrefs.GetFloat(reputationSumKey, 0f);
        totalRatings = PlayerPrefs.GetInt(totalRatingsKey, 0);
    }

    private void NotifyReputationChanged()
    {
        OnReputationChanged?.Invoke(currentReputation);
    }

    [ContextMenu("Resetar Reputação Salva")]
    private void ResetSavedReputation()
    {
        ratingSum = 0f;
        totalRatings = 0;
        currentReputation = 0f;

        SaveReputation();
        NotifyReputationChanged();

        Debug.Log("[GlobalReputationSystem] Reputação salva resetada.");
    }
}