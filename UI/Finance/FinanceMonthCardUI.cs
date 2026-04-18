using TMPro;
using UnityEngine;

public class FinanceMonthCardUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Transform rowsContainer;
    [SerializeField] private FinanceMovementRowUI rowPrefab;

    public void Setup(FinanceMonthGroupData group)
    {
        if (group == null)
            return;

        if (titleText != null)
            titleText.text = group.GetMonthYearText();

        if (rowsContainer == null || rowPrefab == null)
            return;

        ClearRows();

        for (int i = 0; i < group.Movements.Count; i++)
        {
            FinanceMovementRowUI row = Instantiate(rowPrefab, rowsContainer);
            row.Setup(group.Movements[i]);
        }
    }

    private void ClearRows()
    {
        if (rowsContainer == null)
            return;

        for (int i = rowsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(rowsContainer.GetChild(i).gameObject);
        }
    }
}