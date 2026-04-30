using System.Collections.Generic;
using UnityEngine;

public class AppointmentPanelUI : BootstrapUIBehaviour
{
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private Transform contentParent;
    [SerializeField] private AppointmentItemUI itemPrefab;

    [Header("Configuração")]
    [SerializeField] private bool refreshWhenInitialized = true;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    private readonly List<AppointmentItemUI> items = new List<AppointmentItemUI>();
    private bool subscribed;

    protected override void OnBootstrapInitialize()
    {
        Subscribe();

        if (enableDebugLogs)
            Debug.Log("[AppointmentPanelUI] Inicializado pelo GameBootstrap.");

        if (refreshWhenInitialized)
            Refresh();
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
        {
            if (enableDebugLogs)
                Debug.LogWarning("[AppointmentPanelUI] ClientAppointmentScheduler.Instance não encontrado.");

            return;
        }

        ClientAppointmentScheduler.Instance.OnAppointmentsChanged -= Refresh;
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
            if (enableDebugLogs)
                Debug.LogWarning("[AppointmentPanelUI] Content Parent não configurado.");

            return;
        }

        if (itemPrefab == null)
        {
            if (enableDebugLogs)
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