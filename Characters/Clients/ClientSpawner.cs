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
    [SerializeField] private bool autoSpawn = true;
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

        return GameTimeSystem.Instance.IsWorkDay && GameTimeSystem.Instance.IsWithinBusinessHours;
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

        ConfigureSpawnedClient(client, prefabToSpawn);

        aliveClients.Add(client);

        if (preventDuplicatePrefabAlive)
            activePrefabTypes.Add(prefabToSpawn);

        return true;
    }

    private void ConfigureSpawnedClient(ClientNPC client, GameObject sourcePrefab)
    {
        if (client == null)
            return;

        client.SetPlayerTransform(playerTransform);

        ClientRequestData fallbackRequest = GetFallbackRequestIfNeeded(client);

        client.Initialize(
            this,
            waitingAreaManager,
            entrancePoint,
            null,
            exitPoint,
            sourcePrefab,
            fallbackRequest
        );

        LogClientConfiguration(client, fallbackRequest);
    }

    private ClientRequestData GetFallbackRequestIfNeeded(ClientNPC client)
    {
        if (client == null)
            return null;

        // Se o cliente já tiver perfil fixo com request definido,
        // o fallback não precisa ser usado.
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