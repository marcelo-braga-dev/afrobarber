using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FinanceMonthCardUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Transform rowsContainer;
    [SerializeField] private FinanceMovementRowUI rowPrefab;

    [Header("Paginação de Movimentações")]
    [SerializeField] private int rowsPerPage = 15;

    private FinanceMonthGroupData currentGroup;
    private readonly List<FinanceMovementRowUI> spawnedRows = new List<FinanceMovementRowUI>();

    private int currentLoadedIndex;
    private bool isLoadingRows;

    public void Setup(FinanceMonthGroupData group)
    {
        currentGroup = group;
        currentLoadedIndex = 0;
        isLoadingRows = false;

        ClearRows();

        if (currentGroup == null)
            return;

        if (titleText != null)
            titleText.text = currentGroup.GetMonthYearText();

        LoadNextRowsPage();
    }

    public bool TryLoadMoreRows()
    {
        if (!HasMoreRowsToLoad())
            return false;

        LoadNextRowsPage();
        return true;
    }

    public bool HasMoreRowsToLoad()
    {
        return currentGroup != null &&
               currentGroup.Movements != null &&
               currentLoadedIndex < currentGroup.Movements.Count;
    }

    public void LoadNextRowsPage()
    {
        if (isLoadingRows)
            return;

        if (!HasMoreRowsToLoad())
            return;

        if (rowsContainer == null || rowPrefab == null)
            return;

        isLoadingRows = true;

        int safeRowsPerPage = Mathf.Max(1, rowsPerPage);
        int endIndex = Mathf.Min(currentLoadedIndex + safeRowsPerPage, currentGroup.Movements.Count);

        for (int i = currentLoadedIndex; i < endIndex; i++)
        {
            FinanceMovementRowUI row = Instantiate(rowPrefab, rowsContainer);
            row.Setup(currentGroup.Movements[i]);
            spawnedRows.Add(row);
        }

        currentLoadedIndex = endIndex;
        isLoadingRows = false;

        RectTransform rect = rowsContainer as RectTransform;
        if (rect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
    }

    private void ClearRows()
    {
        if (rowsContainer != null)
        {
            for (int i = rowsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(rowsContainer.GetChild(i).gameObject);
            }
        }

        spawnedRows.Clear();
    }
}