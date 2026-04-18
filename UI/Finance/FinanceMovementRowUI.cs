using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FinanceMovementRowUI : MonoBehaviour
{
    [Header("Textos")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text originText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private TMP_Text statusText;

    [Header("Ações")]
    [SerializeField] private Button payButton;
    [SerializeField] private GameObject paidIcon;
    [SerializeField] private GameObject blockedIcon;

    [Header("Cores")]
    [SerializeField] private Color incomeColor = new Color(0.14f, 0.75f, 0.30f, 1f);
    [SerializeField] private Color expenseColor = new Color(0.90f, 0.22f, 0.22f, 1f);

    private FinanceMovementData currentData;

    public void Setup(FinanceMovementData data)
    {
        currentData = data;

        if (data == null)
            return;

        DateTime referenceDate = data.IsExpense && data.HasDueDate ? data.DueDate : data.CreatedAt;
        DateTime now = FinanceManager.Instance != null ? FinanceManager.Instance.GetNow() : DateTime.Now;

        if (dayText != null)
            dayText.text = referenceDate.ToString("dd");

        if (titleText != null)
            titleText.text = data.title;

        if (descriptionText != null)
            descriptionText.text = string.IsNullOrWhiteSpace(data.description) ? "-" : data.description;

        if (originText != null)
            originText.text = GetOriginLabel(data.origin);

        if (dateText != null)
        {
            string label = data.IsIncome
                ? $"Entrada: {referenceDate:dd/MM/yyyy}"
                : $"Venc.: {referenceDate:dd/MM/yyyy}";

            dateText.text = label;
        }

        if (amountText != null)
        {
            amountText.text = $"R$ {data.amount}";
            amountText.color = data.IsIncome ? incomeColor : expenseColor;
        }

        if (statusText != null)
            statusText.text = GetStatusLabel(data, now);

        if (payButton != null)
        {
            payButton.onClick.RemoveAllListeners();
            payButton.gameObject.SetActive(CanShowPayButton(data, now));

            if (payButton.gameObject.activeSelf)
                payButton.onClick.AddListener(OnClickPay);
        }

        if (paidIcon != null)
            paidIcon.SetActive(data.IsExpense && data.IsPaid);

        if (blockedIcon != null)
            blockedIcon.SetActive(ShouldShowBlockedIcon(data, now));
    }

    private bool CanShowPayButton(FinanceMovementData data, DateTime now)
    {
        if (data == null || !data.IsExpense || data.IsPaid)
            return false;

        return data.IsPayable(now);
    }

    private bool ShouldShowBlockedIcon(FinanceMovementData data, DateTime now)
    {
        if (data == null || !data.IsExpense)
            return false;

        if (data.IsPaid)
            return false;

        return !data.IsPayable(now);
    }

    private string GetStatusLabel(FinanceMovementData data, DateTime now)
    {
        if (data.IsIncome)
            return "Recebido";

        if (data.IsPaid)
            return "Pago";

        if (data.IsFutureExpense(now))
            return "Futuro";

        if (data.IsOverdue(now))
            return "Atrasado";

        if (data.IsPayable(now))
            return "Em aberto";

        return "Indisponível";
    }

    private string GetOriginLabel(FinanceMovementOrigin origin)
    {
        switch (origin)
        {
            case FinanceMovementOrigin.Service: return "Atendimento";
            case FinanceMovementOrigin.ProductPurchase: return "Compra";
            case FinanceMovementOrigin.Rent: return "Aluguel";
            case FinanceMovementOrigin.Electricity: return "Energia";
            case FinanceMovementOrigin.Water: return "Água";
            case FinanceMovementOrigin.Salary: return "Salário";
            case FinanceMovementOrigin.Maintenance: return "Manutenção";
            case FinanceMovementOrigin.Supply: return "Insumos";
            case FinanceMovementOrigin.Tax: return "Taxa";
            default: return "Outros";
        }
    }

    private void OnClickPay()
    {
        if (currentData == null || FinanceManager.Instance == null)
            return;

        bool success = FinanceManager.Instance.PayExpense(currentData.id);

        if (!success)
            Debug.LogWarning("[FinanceMovementRowUI] Não foi possível pagar a despesa.");
    }
}