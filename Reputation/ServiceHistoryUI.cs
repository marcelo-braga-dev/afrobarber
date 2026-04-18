using System.Collections.Generic;
using UnityEngine;

public class ServiceHistoryUI : MonoBehaviour
{
    [Header("Painel")]
    [SerializeField] private GameObject historyPanel;

    [Header("Lista")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private ServiceHistoryItemUI historyItemPrefab;

    private readonly List<ServiceHistoryItemUI> spawnedItems = new List<ServiceHistoryItemUI>();

    private void OnEnable()
    {
        if (ServiceHistorySystem.Instance != null)
        {
            ServiceHistorySystem.Instance.OnHistoryChanged += RefreshHistoryUI;
        }

        RefreshHistoryUI();
    }

    private void OnDisable()
    {
        if (ServiceHistorySystem.Instance != null)
        {
            ServiceHistorySystem.Instance.OnHistoryChanged -= RefreshHistoryUI;
        }
    }

    public void ToggleHistoryPanel()
    {
        if (historyPanel == null)
            return;

        historyPanel.SetActive(!historyPanel.activeSelf);

        if (historyPanel.activeSelf)
        {
            RefreshHistoryUI();
        }
    }

    public void OpenHistoryPanel()
    {
        if (historyPanel == null)
            return;

        historyPanel.SetActive(true);
        RefreshHistoryUI();
    }

    public void CloseHistoryPanel()
    {
        if (historyPanel == null)
            return;

        historyPanel.SetActive(false);
    }

    public void RefreshHistoryUI()
    {
        if (contentParent == null || historyItemPrefab == null)
            return;

        ClearItems();

        if (ServiceHistorySystem.Instance == null)
            return;

        var entries = ServiceHistorySystem.Instance.Entries;

        for (int i = entries.Count - 1; i >= 0; i--)
        {
            ServiceHistoryItemUI item = Instantiate(historyItemPrefab, contentParent);
            item.Setup(entries[i]);
            spawnedItems.Add(item);
        }
    }

    private void ClearItems()
    {
        for (int i = 0; i < spawnedItems.Count; i++)
        {
            if (spawnedItems[i] != null)
            {
                Destroy(spawnedItems[i].gameObject);
            }
        }

        spawnedItems.Clear();
    }
}