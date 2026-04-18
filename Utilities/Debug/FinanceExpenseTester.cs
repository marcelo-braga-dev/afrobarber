using System;
using UnityEngine;

public class FinanceExpenseTester : MonoBehaviour
{
    [Header("Dados da despesa")]
    [SerializeField] private string title = "Conta de energia";
    [TextArea][SerializeField] private string description = "Despesa cadastrada manualmente";
    [SerializeField] private FinanceMovementOrigin origin = FinanceMovementOrigin.Electricity;
    [SerializeField] private int amount = 250;

    [Header("Vencimento")]
    [SerializeField] private int year = 2026;
    [SerializeField] private int month = 4;
    [SerializeField] private int day = 15;

    [ContextMenu("Cadastrar Despesa")]
    public void CreateExpense()
    {
        if (FinanceManager.Instance == null)
        {
            Debug.LogWarning("FinanceManager.Instance não encontrado.");
            return;
        }

        DateTime dueDate = new DateTime(year, month, day, 8, 0, 0);

        FinanceManager.Instance.AddExpense(
            title,
            description,
            origin,
            amount,
            dueDate
        );
    }
}