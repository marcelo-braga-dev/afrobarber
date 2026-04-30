using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReputationUI : BootstrapUIBehaviour
{
    [Header("Referências")]
    [SerializeField] private TMP_Text reputationText;
    [SerializeField] private TMP_Text totalRatingsText;
    [SerializeField] private Slider reputationSlider;

    [Header("Debug")]
    [SerializeField] private bool enableLogs = false;

    private readonly CultureInfo brazilCulture = new CultureInfo("pt-BR");

    protected override void OnBootstrapInitialize()
    {
        SetupSlider();
        TryBindAndRefresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void SetupSlider()
    {
        if (reputationSlider == null)
        {
            LogWarning("reputationSlider não foi configurado.");
            return;
        }

        reputationSlider.minValue = 0f;
        reputationSlider.maxValue = 5f;
        reputationSlider.wholeNumbers = false;
        reputationSlider.value = 0f;
    }

    private void TryBindAndRefresh()
    {
        if (GlobalReputationSystem.Instance == null)
        {
            LogWarning("GlobalReputationSystem.Instance não encontrado.");
            UpdateUI(0f);
            return;
        }

        Unsubscribe();

        GlobalReputationSystem.Instance.OnReputationChanged += UpdateUI;

        Log("Conectado ao GlobalReputationSystem.");
        UpdateUI(GlobalReputationSystem.Instance.Reputation);
    }

    private void Unsubscribe()
    {
        if (GlobalReputationSystem.Instance != null)
            GlobalReputationSystem.Instance.OnReputationChanged -= UpdateUI;
    }

    private void UpdateUI(float reputationValue)
    {
        reputationValue = Mathf.Clamp(reputationValue, 0f, 5f);
        reputationValue = Mathf.Round(reputationValue * 10f) / 10f;

        if (reputationText != null)
            reputationText.text = $"{reputationValue.ToString("0.0", brazilCulture)}/5";
        else
            LogWarning("reputationText não foi configurado.");

        if (totalRatingsText != null && GlobalReputationSystem.Instance != null)
            totalRatingsText.text = $"Avaliações: {GlobalReputationSystem.Instance.TotalRatings}";

        if (reputationSlider != null)
            reputationSlider.value = reputationValue;
        else
            LogWarning("reputationSlider está null.");

        Log($"UI atualizada: {reputationValue.ToString("0.0", brazilCulture)}/5");
    }

    private void Log(string message)
    {
        if (enableLogs)
            Debug.Log("[ReputationUI] " + message);
    }

    private void LogWarning(string message)
    {
        if (enableLogs)
            Debug.LogWarning("[ReputationUI] " + message);
    }
}