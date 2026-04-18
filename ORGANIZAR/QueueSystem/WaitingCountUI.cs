using TMPro;
using UnityEngine;
using System.Collections;

public class WaitingCountUI : MonoBehaviour
{
    [SerializeField] private TMP_Text waitingCountText;
    [SerializeField] private string prefix = "Clientes aguardando: ";

    private bool isSubscribed;

    private void Start()
    {
        StartCoroutine(TryBindQueueSystemRoutine());
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    private void OnDisable()
    {
        UnbindQueueSystem();
    }

    private IEnumerator TryBindQueueSystemRoutine()
    {
        while (BarberQueueSystem.Instance == null)
        {
            yield return null;
        }

        BindQueueSystem();
        RefreshUI();
    }

    private void BindQueueSystem()
    {
        if (isSubscribed)
            return;

        if (BarberQueueSystem.Instance == null)
            return;

        BarberQueueSystem.Instance.OnQueueChanged += RefreshUI;
        isSubscribed = true;

        Debug.Log("[WaitingCountUI] Conectado ao BarberQueueSystem.");
    }

    private void UnbindQueueSystem()
    {
        if (!isSubscribed)
            return;

        if (BarberQueueSystem.Instance != null)
        {
            BarberQueueSystem.Instance.OnQueueChanged -= RefreshUI;
        }

        isSubscribed = false;
    }

    public void RefreshUI()
    {
        if (waitingCountText == null)
        {
            Debug.LogWarning("[WaitingCountUI] waitingCountText não foi configurado.");
            return;
        }

        int count = 0;

        if (BarberQueueSystem.Instance != null)
        {
            count = BarberQueueSystem.Instance.GetWaitingCount();
        }
        else
        {
            Debug.LogWarning("[WaitingCountUI] BarberQueueSystem.Instance não encontrado.");
        }

        waitingCountText.text = $"{prefix}{count}";
        Debug.Log($"[WaitingCountUI] Texto atualizado: {count}");
    }
}