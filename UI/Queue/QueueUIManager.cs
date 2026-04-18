using System.Collections.Generic;
using UnityEngine;

public class QueueUIManager : MonoBehaviour
{
    [Header("Painel")]
    [SerializeField] private GameObject queuePanel;

    [Header("Lista")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private QueueClientCardUI cardPrefab;

    [Header("Ordenação")]
    [SerializeField] private bool orderByUrgency = false;

    private readonly List<QueueClientCardUI> spawnedCards = new List<QueueClientCardUI>();

    private void OnEnable()
    {
        if (BarberQueueSystem.Instance != null)
        {
            BarberQueueSystem.Instance.OnQueueChanged += RefreshQueueUI;
        }

        RefreshQueueUI();
    }

    private void OnDisable()
    {
        if (BarberQueueSystem.Instance != null)
        {
            BarberQueueSystem.Instance.OnQueueChanged -= RefreshQueueUI;
        }
    }

    public void ToggleQueuePanel()
    {
        if (queuePanel == null)
            return;

        queuePanel.SetActive(!queuePanel.activeSelf);

        if (queuePanel.activeSelf)
        {
            RefreshQueueUI();
        }
    }

    public void OpenQueuePanel()
    {
        if (queuePanel == null)
            return;

        queuePanel.SetActive(true);
        RefreshQueueUI();
    }

    public void CloseQueuePanel()
    {
        if (queuePanel == null)
            return;

        queuePanel.SetActive(false);
    }

    public void RefreshQueueUI()
    {
        if (contentParent == null || cardPrefab == null)
            return;

        ClearCards();

        if (BarberQueueSystem.Instance == null)
            return;

        List<ClientQueueData> clients = orderByUrgency
            ? BarberQueueSystem.Instance.GetQueueOrderedByUrgency()
            : BarberQueueSystem.Instance.GetQueueOrderedByArrival();

        for (int i = 0; i < clients.Count; i++)
        {
            QueueClientCardUI card = Instantiate(cardPrefab, contentParent);
            card.Setup(clients[i]);
            spawnedCards.Add(card);
        }
    }

    private void ClearCards()
    {
        for (int i = 0; i < spawnedCards.Count; i++)
        {
            if (spawnedCards[i] != null)
                Destroy(spawnedCards[i].gameObject);
        }

        spawnedCards.Clear();
    }
}