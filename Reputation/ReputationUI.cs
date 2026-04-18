using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReputationUI : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private TMP_Text reputationText;
    [SerializeField] private TMP_Text totalRatingsText;
    [SerializeField] private Slider reputationSlider;

    private readonly CultureInfo brazilCulture = new CultureInfo("pt-BR");

    private void Start()
    {
        SetupSlider();
        TryBindAndRefresh();
    }

    private void OnEnable()
    {
        TryBindAndRefresh();
    }

    private void OnDisable()
    {
        if (GlobalReputationSystem.Instance != null)
        {
            GlobalReputationSystem.Instance.OnReputationChanged -= UpdateUI;
        }
    }

    private void SetupSlider()
    {
        if (reputationSlider == null)
        {
            Debug.LogWarning("[ReputationUI] reputationSlider não foi configurado.");
            return;
        }

        reputationSlider.minValue = 0f;
        reputationSlider.maxValue = 5f;
        reputationSlider.wholeNumbers = false;
        reputationSlider.value = 0f;
    }

    private void TryBindAndRefresh()
    {
        if (GlobalReputationSystem.Instance != null)
        {
            GlobalReputationSystem.Instance.OnReputationChanged -= UpdateUI;
            GlobalReputationSystem.Instance.OnReputationChanged += UpdateUI;

            Debug.Log("[ReputationUI] Conectado ao GlobalReputationSystem.");
            UpdateUI(GlobalReputationSystem.Instance.Reputation);
        }
        else
        {
            Debug.LogWarning("[ReputationUI] GlobalReputationSystem.Instance não encontrado.");
            UpdateUI(0f);
        }
    }

    private void UpdateUI(float reputationValue)
    {
        reputationValue = Mathf.Clamp(reputationValue, 0f, 5f);
        reputationValue = Mathf.Round(reputationValue * 10f) / 10f;

        if (reputationText != null)
        {
            reputationText.text = $"{reputationValue.ToString("0.0", brazilCulture)}/5";
        }
        else
        {
            Debug.LogWarning("[ReputationUI] reputationText não foi configurado.");
        }

        if (totalRatingsText != null && GlobalReputationSystem.Instance != null)
        {
            totalRatingsText.text = $"Avaliações: {GlobalReputationSystem.Instance.TotalRatings}";
        }

        if (reputationSlider != null)
        {
            reputationSlider.value = reputationValue;
        }
        else
        {
            Debug.LogWarning("[ReputationUI] reputationSlider está null.");
        }

        Debug.Log($"[ReputationUI] UI atualizada: {reputationValue}/5");
    }
}