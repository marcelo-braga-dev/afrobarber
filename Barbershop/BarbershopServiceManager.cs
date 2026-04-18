using UnityEngine;

public class BarbershopServiceManager : MonoBehaviour
{
    public static BarbershopServiceManager Instance { get; private set; }

    [SerializeField] private Transform barberChairWalkPoint;
    [SerializeField] private Transform barberChairSitPoint;
    [SerializeField] private Transform cashierPoint;
    [SerializeField] private Transform exitPoint;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private ClientNPC currentClient;

    public Transform BarberChairWalkPoint => barberChairWalkPoint;
    public Transform BarberChairSitPoint => barberChairSitPoint;
    public Transform CashierPoint => cashierPoint;
    public Transform ExitPoint => exitPoint;
    public ClientNPC CurrentClient => currentClient;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool HasClientInService()
    {
        return currentClient != null;
    }

    public bool TryStartService(ClientNPC client)
    {
        if (client == null)
            return false;

        if (PlayerEnergySystem.Instance != null && !PlayerEnergySystem.Instance.CanStartService())
        {
            Debug.LogWarning("[BarbershopServiceManager] Energia abaixo de 10. Não é possível iniciar atendimento.");
            return false;
        }

        if (currentClient != null)
        {
            Debug.LogWarning("[BarbershopServiceManager] Já existe um cliente em atendimento.");
            return false;
        }

        currentClient = client;

        if (enableDebugLogs)
            Debug.Log($"[BarbershopServiceManager] Iniciando atendimento de {client.name}");

        client.StartService(barberChairWalkPoint, barberChairSitPoint);
        return true;
    }

    public void CompleteCurrentService()
    {
        if (currentClient == null)
        {
            Debug.LogWarning("[BarbershopServiceManager] Não existe cliente atual para concluir atendimento.");
            return;
        }

        ClientRequestData request = currentClient.RequestData;

        if (enableDebugLogs)
        {
            Debug.Log(
                $"[BarbershopServiceManager] Concluindo atendimento de {currentClient.name} | " +
                $"Pedido: {(request != null ? request.requestName : "NULL")}"
            );
        }

        currentClient.MarkServiceCompleted();

        if (request != null)
        {
            if (EducationProgressManager.Instance != null && !string.IsNullOrWhiteSpace(request.afroCutId))
            {
                EducationProgressManager.Instance.UnlockCut(request.afroCutId);
            }

            // Aqui você pode integrar depois com dinheiro/XP reais do jogador:
            // FinanceManager.Instance?.AddMoney(request.ServicePrice);
            // PlayerLevelSystem.Instance?.AddXP(request.xpReward);
        }

        if (cashierPoint != null)
        {
            currentClient.GoToCashier(cashierPoint);
        }
        else if (exitPoint != null)
        {
            currentClient.LeaveShop(exitPoint);
        }
        else
        {
            currentClient.ForceDespawn();
        }
    }

    public void NotifyClientFinishedCashier(ClientNPC client)
    {
        if (client == null)
            return;

        if (client == currentClient)
        {
            currentClient = null;

            if (enableDebugLogs)
                Debug.Log($"[BarbershopServiceManager] Cliente {client.name} terminou no caixa.");
        }

        if (exitPoint != null)
            client.LeaveShop(exitPoint);
        else
            client.ForceDespawn();
    }

    public void ClearCurrentClient(ClientNPC client)
    {
        if (client == null)
            return;

        if (currentClient == client)
        {
            currentClient = null;

            if (enableDebugLogs)
                Debug.Log($"[BarbershopServiceManager] Cliente {client.name} liberado do atendimento.");
        }
    }
}