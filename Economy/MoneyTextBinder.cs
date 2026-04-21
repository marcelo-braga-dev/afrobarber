using TMPro;
using UnityEngine;

public class MoneyTextBinder : MonoBehaviour
{
    [SerializeField] private TMP_Text targetText;
    [SerializeField] private string prefix = "R$ ";

    private void OnEnable()
    {
        if (FinanceManager.Instance != null)
        {
            FinanceManager.Instance.OnCashChanged += HandleCashChanged;
            HandleCashChanged(FinanceManager.Instance.CurrentCash);
        }
    }

    private void OnDisable()
    {
        if (FinanceManager.Instance != null)
        {
            FinanceManager.Instance.OnCashChanged -= HandleCashChanged;
        }
    }

    private void HandleCashChanged(int value)
    {
        if (targetText != null)
            targetText.text = $"{prefix}{value}";
    }
}