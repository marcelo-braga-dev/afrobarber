using TMPro;
using UnityEngine;

public class WaitingCountUI : BootstrapUIBehaviour
{
    [SerializeField] private TMP_Text waitingCountText;
    [SerializeField] private string prefix = "Clientes aguardando: ";

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    private bool isSubscribed;

    protected override void OnBootstrapInitialize()
    {
        BindQueueSystem();
        RefreshUI();
    }

    private void OnDisable()
    {
        UnbindQueueSystem();
    }

    private void BindQueueSystem()
    {
        if (isSubscribed)
            return;

        if (BarberQueueSystem.Instance == null)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[WaitingCountUI] BarberQueueSystem.Instance não encontrado.");

            return;
        }

        BarberQueueSystem.Instance.OnQueueChanged -= RefreshUI;
        BarberQueueSystem.Instance.OnQueueChanged += RefreshUI;

        isSubscribed = true;

        if (enableDebugLogs)
            Debug.Log("[WaitingCountUI] Conectado ao BarberQueueSystem.");
    }

    private void UnbindQueueSystem()
    {
        if (!isSubscribed)
            return;

        if (BarberQueueSystem.Instance != null)
            BarberQueueSystem.Instance.OnQueueChanged -= RefreshUI;

        isSubscribed = false;
    }

    public void RefreshUI()
    {
        if (waitingCountText == null)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[WaitingCountUI] waitingCountText não foi configurado.");

            return;
        }

        int count = 0;

        if (BarberQueueSystem.Instance != null)
            count = BarberQueueSystem.Instance.GetWaitingCount();

        waitingCountText.text = $"{prefix}{count}";

        if (enableDebugLogs)
            Debug.Log($"[WaitingCountUI] Texto atualizado: {count}");
    }
}