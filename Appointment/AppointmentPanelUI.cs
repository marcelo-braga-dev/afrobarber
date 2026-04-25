using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppointmentPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private Transform contentParent;
    [SerializeField] private AppointmentItemUI itemPrefab;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private readonly List<AppointmentItemUI> items = new List<AppointmentItemUI>();
    private bool subscribed;

    private void Start()
    {
        StartCoroutine(WaitForScheduler());
    }

    private IEnumerator WaitForScheduler()
    {
        while (ClientAppointmentScheduler.Instance == null)
            yield return null;

        Subscribe();

        if (enableDebugLogs)
            Debug.Log("[AppointmentPanelUI] ClientAppointmentScheduler encontrado.");

        Refresh();
    }

    private void OnEnable()
    {
        if (ClientAppointmentScheduler.Instance != null)
        {
            Subscribe();
            Refresh();
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (subscribed)
            return;

        if (ClientAppointmentScheduler.Instance == null)
            return;

        ClientAppointmentScheduler.Instance.OnAppointmentsChanged += Refresh;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed)
            return;

        if (ClientAppointmentScheduler.Instance != null)
            ClientAppointmentScheduler.Instance.OnAppointmentsChanged -= Refresh;

        subscribed = false;
    }

    public void Toggle()
    {
        if (rootPanel != null)
            rootPanel.SetActive(!rootPanel.activeSelf);

        Refresh();
    }

    public void Open()
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);

        Refresh();
    }

    public void Close()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    public void Refresh()
    {
        Clear();

        if (ClientAppointmentScheduler.Instance == null)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[AppointmentPanelUI] ClientAppointmentScheduler.Instance não encontrado.");

            return;
        }

        if (contentParent == null)
        {
            Debug.LogWarning("[AppointmentPanelUI] Content Parent não configurado.");
            return;
        }

        if (itemPrefab == null)
        {
            Debug.LogWarning("[AppointmentPanelUI] Item Prefab não configurado.");
            return;
        }

        int total = ClientAppointmentScheduler.Instance.Appointments.Count;

        if (enableDebugLogs)
            Debug.Log($"[AppointmentPanelUI] Atualizando agenda. Total: {total}");

        foreach (ClientAppointmentData appointment in ClientAppointmentScheduler.Instance.Appointments)
        {
            AppointmentItemUI item = Instantiate(itemPrefab, contentParent);
            item.gameObject.SetActive(true);
            item.Setup(appointment);
            items.Add(item);
        }
    }

    private void Clear()
    {
        if (contentParent == null)
            return;

        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        items.Clear();
    }

    [ContextMenu("TESTE - Atualizar UI da Agenda")]
    private void TestRefresh()
    {
        Refresh();
    }
}