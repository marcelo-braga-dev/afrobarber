using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSpawner : MonoBehaviour
{
    [Header("Prefabs disponíveis")]
    [SerializeField] private List<GameObject> clientPrefabs = new List<GameObject>();

    [Header("Player")]
    [SerializeField] private Transform playerTransform;

    [Header("Banco de serviços (fallback)")]
    [SerializeField] private ServiceDatabase serviceDatabase;

    [Header("Referências")]
    [SerializeField] private WaitingAreaManager waitingAreaManager;
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private Transform entrancePoint;
    [SerializeField] private Transform exitPoint;

    [Header("Spawn")]
    [SerializeField] private bool autoSpawn = false;
    [SerializeField] private float minSpawnDelay = 4f;
    [SerializeField] private float maxSpawnDelay = 10f;
    [SerializeField] private int maxClientsAlive = 10;

    [Header("Regras")]
    [SerializeField] private bool preventDuplicatePrefabAlive = true;
    [SerializeField] private bool requireValidClientNPC = true;

    private readonly List<ClientNPC> aliveClients = new List<ClientNPC>();
    private readonly HashSet<GameObject> activePrefabTypes = new HashSet<GameObject>();

    private Coroutine spawnRoutine;
    private bool clientsDismissedForClosedHours;
    private float demandMultiplier = 1f;

    private void Awake()
    {
        AutoFindReferences();
    }

    private void Start()
    {
        if (autoSpawn)
            spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private void Update()
    {
        CleanupDestroyedClients();

        if (GameTimeSystem.Instance == null)
            return;

        bool isOpen = GameTimeSystem.Instance.IsWorkDay && GameTimeSystem.Instance.IsWithinBusinessHours;

        if (!isOpen)
        {
            if (!clientsDismissedForClosedHours)
            {
                DismissAllClientsDueToClosingTime();
                clientsDismissedForClosedHours = true;
            }
        }
        else
        {
            clientsDismissedForClosedHours = false;
        }
    }

    private void AutoFindReferences()
    {
        if (waitingAreaManager == null)
            waitingAreaManager = FindFirstObjectByType<WaitingAreaManager>();

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
                playerTransform = player.transform;
        }

        if (exitPoint == null && BarbershopServiceManager.Instance != null)
            exitPoint = BarbershopServiceManager.Instance.ExitPoint;
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);

            if (CanSpawnNow())
                TrySpawnClient();
        }
    }

    private bool CanSpawnNow()
    {
        if (GameTimeSystem.Instance == null)
            return true;

        bool baseOpen = GameTimeSystem.Instance.IsWorkDay && GameTimeSystem.Instance.IsWithinBusinessHours;

        if (!baseOpen)
            return false;

        float spawnChance = Mathf.Clamp(demandMultiplier, 0.2f, 2f);

        if (spawnChance >= 1f)
            return true;

        return Random.value <= spawnChance;
    }

    public bool TrySpawnClient()
    {
        CleanupDestroyedClients();

        if (!CanSpawnNow())
            return false;

        if (!ValidateReferences())
            return false;

        if (aliveClients.Count >= maxClientsAlive)
            return false;

        GameObject prefabToSpawn = GetAvailablePrefab();

        if (prefabToSpawn == null)
        {
            Debug.Log("[ClientSpawner] Nenhum prefab disponível para spawn.");
            return false;
        }

        Transform selectedSpawnPoint = GetAvailableSpawnPoint();

        if (selectedSpawnPoint == null)
        {
            Debug.LogWarning("[ClientSpawner] Nenhum spawn point válido foi encontrado.");
            return false;
        }

        GameObject instance = Instantiate(
            prefabToSpawn,
            selectedSpawnPoint.position,
            selectedSpawnPoint.rotation
        );

        ClientNPC client = instance.GetComponent<ClientNPC>();

        if (client == null)
        {
            if (requireValidClientNPC)
            {
                Debug.LogWarning($"[ClientSpawner] O prefab {prefabToSpawn.name} não possui ClientNPC.");
                Destroy(instance);
                return false;
            }

            return false;
        }

        ConfigureSpawnedClient(client, prefabToSpawn, null);

        return true;
    }

    public bool TrySpawnScheduledClient(
    GameObject prefab,
    ClientRequestData requestOverride,
    out ClientNPC spawnedClient
)
    {
        spawnedClient = null;

        CleanupDestroyedClients();

        if (!ValidateReferences())
            return false;

        if (aliveClients.Count >= maxClientsAlive)
            return false;

        if (prefab == null)
            return false;

        if (preventDuplicatePrefabAlive && activePrefabTypes.Contains(prefab))
            return false;

        Transform selectedSpawnPoint = GetAvailableSpawnPoint();

        if (selectedSpawnPoint == null)
            return false;

        GameObject instance = Instantiate(
            prefab,
            selectedSpawnPoint.position,
            selectedSpawnPoint.rotation
        );

        ClientNPC client = instance.GetComponent<ClientNPC>();

        if (client == null)
        {
            Destroy(instance);
            return false;
        }

        ConfigureSpawnedClient(client, prefab, requestOverride);

        spawnedClient = client;

        Debug.Log($"[ClientSpawner] Cliente spawnado via agenda: {prefab.name}");

        return true;
    }

    private void ConfigureSpawnedClient(ClientNPC client, GameObject sourcePrefab, ClientRequestData requestOverride)
    {
        if (client == null)
            return;

        client.SetPlayerTransform(playerTransform);

        ClientRequestData fallbackRequest = requestOverride != null
            ? requestOverride
            : GetFallbackRequestIfNeeded(client);

        client.Initialize(
            this,
            waitingAreaManager,
            entrancePoint,
            null,
            exitPoint,
            sourcePrefab,
            fallbackRequest
        );

        aliveClients.Add(client);

        if (preventDuplicatePrefabAlive)
            activePrefabTypes.Add(sourcePrefab);

        LogClientConfiguration(client, fallbackRequest);
    }

    private ClientRequestData GetFallbackRequestIfNeeded(ClientNPC client)
    {
        if (client == null)
            return null;

        if (client.HasFixedProfileRequest())
            return null;

        if (serviceDatabase == null)
            return null;

        ClientRequestData randomService = serviceDatabase.GetRandomService();

        if (randomService == null)
        {
            Debug.LogWarning("[ClientSpawner] Nenhum serviço retornado pelo ServiceDatabase.");
            return null;
        }

        return randomService;
    }

    private void LogClientConfiguration(ClientNPC client, ClientRequestData fallbackRequest)
    {
        if (client == null)
            return;

        if (client.HasFixedProfileRequest())
        {
            ClientRequestData request = client.CurrentRequest;

            if (request != null)
            {
                Debug.Log(
                    $"[ClientSpawner] Cliente {client.name} spawnado com perfil fixo. " +
                    $"Pedido: {request.RequestName}"
                );
            }
            else
            {
                Debug.Log(
                    $"[ClientSpawner] Cliente {client.name} spawnado com perfil fixo, mas sem request configurado."
                );
            }

            return;
        }

        if (fallbackRequest != null)
        {
            Debug.Log(
                $"[ClientSpawner] Cliente {client.name} spawnado com serviço fallback. " +
                $"Pedido: {fallbackRequest.RequestName}"
            );
        }
        else
        {
            Debug.LogWarning(
                $"[ClientSpawner] Cliente {client.name} spawnado sem request fixo e sem fallback."
            );
        }
    }

    public void UpdateDemandMultiplierFromServicePrice(int finalPrice, int suggestedPrice)
    {
        if (GlobalGameplayManagement.Instance == null)
        {
            demandMultiplier = 1f;
            return;
        }

        demandMultiplier = GlobalGameplayManagement.Instance.GetSpawnDemandMultiplierFromLastService(
            finalPrice,
            suggestedPrice
        );
    }

    public void DismissAllClientsDueToClosingTime()
    {
        for (int i = 0; i < aliveClients.Count; i++)
        {
            if (aliveClients[i] != null)
                aliveClients[i].LeaveDueToClosingTime();
        }
    }

    private bool ValidateReferences()
    {
        AutoFindReferences();

        if (playerTransform == null)
        {
            Debug.LogWarning("[ClientSpawner] playerTransform não configurado.");
            return false;
        }

        if (waitingAreaManager == null)
        {
            Debug.LogWarning("[ClientSpawner] waitingAreaManager não configurado.");
            return false;
        }

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("[ClientSpawner] spawnPoints não configurados.");
            return false;
        }

        bool hasAtLeastOneValidSpawn = false;

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (spawnPoints[i] != null)
            {
                hasAtLeastOneValidSpawn = true;
                break;
            }
        }

        if (!hasAtLeastOneValidSpawn)
        {
            Debug.LogWarning("[ClientSpawner] Todos os spawnPoints estão nulos.");
            return false;
        }

        if (entrancePoint == null)
        {
            Debug.LogWarning("[ClientSpawner] entrancePoint não configurado.");
            return false;
        }

        if (exitPoint == null)
        {
            Debug.LogWarning("[ClientSpawner] exitPoint não configurado.");
            return false;
        }

        if (clientPrefabs == null || clientPrefabs.Count == 0)
        {
            Debug.LogWarning("[ClientSpawner] clientPrefabs não configurados.");
            return false;
        }

        return true;
    }

    private Transform GetAvailableSpawnPoint()
    {
        List<Transform> validSpawnPoints = new List<Transform>();

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (spawnPoints[i] != null)
                validSpawnPoints.Add(spawnPoints[i]);
        }

        if (validSpawnPoints.Count == 0)
            return null;

        return validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];
    }

    private GameObject GetAvailablePrefab()
    {
        List<GameObject> available = new List<GameObject>();

        for (int i = 0; i < clientPrefabs.Count; i++)
        {
            GameObject prefab = clientPrefabs[i];

            if (prefab == null)
                continue;

            if (preventDuplicatePrefabAlive)
            {
                if (!activePrefabTypes.Contains(prefab))
                    available.Add(prefab);
            }
            else
            {
                available.Add(prefab);
            }
        }

        if (available.Count == 0)
            return null;

        return available[Random.Range(0, available.Count)];
    }

    public void NotifyClientFinished(ClientNPC client)
    {
        if (client == null)
            return;

        aliveClients.Remove(client);

        if (preventDuplicatePrefabAlive && client.SourcePrefab != null)
            activePrefabTypes.Remove(client.SourcePrefab);

        if (ClientAppointmentScheduler.Instance != null)
            ClientAppointmentScheduler.Instance.NotifyClientFinished(client);
    }

    private void CleanupDestroyedClients()
    {
        for (int i = aliveClients.Count - 1; i >= 0; i--)
        {
            if (aliveClients[i] == null)
                aliveClients.RemoveAt(i);
        }

        activePrefabTypes.Clear();

        if (!preventDuplicatePrefabAlive)
            return;

        for (int i = 0; i < aliveClients.Count; i++)
        {
            if (aliveClients[i] != null && aliveClients[i].SourcePrefab != null)
                activePrefabTypes.Add(aliveClients[i].SourcePrefab);
        }
    }

    public List<ClientNPC> GetAliveClients()
    {
        CleanupDestroyedClients();
        return aliveClients;
    }

    public void ForceSpawnNow()
    {
        TrySpawnClient();
    }

    public void StopAutoSpawn()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    public void StartAutoSpawn()
    {
        if (spawnRoutine == null)
            spawnRoutine = StartCoroutine(SpawnLoop());
    }
}