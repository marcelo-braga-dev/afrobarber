using TMPro;
using UnityEngine;

public class ShopCashHeaderUI : MonoBehaviour
{
    [Header("Texto do caixa")]
    [SerializeField] private TMP_Text cashText;

    [Header("Formato")]
    [SerializeField] private string prefix = "Caixa: R$ ";

    private void OnEnable()
    {
        if (FinanceManager.Instance != null)
        {
            FinanceManager.Instance.OnCashChanged += UpdateCashText;
            UpdateCashText(FinanceManager.Instance.CurrentCash);
        }
        else
        {
            Debug.LogWarning("[ShopCashHeaderUI] FinanceManager.Instance não encontrado.");
            UpdateCashText(0);
        }
    }

    private void OnDisable()
    {
        if (FinanceManager.Instance != null)
            FinanceManager.Instance.OnCashChanged -= UpdateCashText;
    }

    private void Start()
    {
        if (FinanceManager.Instance != null)
            UpdateCashText(FinanceManager.Instance.CurrentCash);
    }

    private void UpdateCashText(int value)
    {
        if (cashText == null)
            return;

        cashText.text = prefix + value.ToString("N0") + ",00";
    }
}