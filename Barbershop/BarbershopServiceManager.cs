using System.Collections;
using UnityEngine;

public class BarbershopServiceManager : MonoBehaviour
{
    public static BarbershopServiceManager Instance { get; private set; }

    [Header("UI de planejamento")]
    [SerializeField] private ServicePlanningUI servicePlanningUI;
    [SerializeField] private bool openPlanningUIBeforeAdvancedExecution = true;

    [Header("Condição para abrir planejamento")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform planningInteractionPoint;
    [SerializeField] private float planningInteractionDistance = 2.5f;
    [SerializeField] private bool requirePlayerNearChairToOpenPlanning = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Pontos do atendimento")]
    [SerializeField] private Transform barberChairWalkPoint;
    [SerializeField] private Transform barberChairSitPoint;
    [SerializeField] private Transform cashierPoint;
    [SerializeField] private Transform exitPoint;

    [Header("Integrações")]
    [SerializeField] private BarberWorkController barberWorkController;

    [Header("Atendimento avançado")]
    [SerializeField] private AdvancedServiceWorkflowManager advancedWorkflow;
    [SerializeField] private bool useAdvancedServiceWorkflow = true;
    [SerializeField] private bool fallbackToOldAutoServiceIfAdvancedFails = true;
    [SerializeField] private bool completeClientVisualFlowAfterAdvancedService = true;

    [Header("Tempos das UIs")]
    [SerializeField] private float advancedExecutionUiCloseDelay = 3f;

    [Header("Configuração do atendimento antigo")]
    [SerializeField] private bool autoCompleteServiceByTime = true;
    [SerializeField] private bool consumeInventoryOnFinish = true;
    [SerializeField] private float defaultEnvironmentComfortScore = 3.5f;
    [SerializeField] private bool defaultHadMistakes = false;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private ClientNPC currentClient;
    private Coroutine currentServiceRoutine;
    private bool currentServiceUsingAdvancedWorkflow;
    private bool waitingForPlayerToOpenPlanning;

    public Transform BarberChairWalkPoint => barberChairWalkPoint;
    public Transform BarberChairSitPoint => barberChairSitPoint;
    public Transform CashierPoint => cashierPoint;
    public Transform ExitPoint => exitPoint;
    public ClientNPC CurrentClient => currentClient;
    public bool HasActiveService => currentClient != null;
    public bool WaitingForPlayerToOpenPlanning => waitingForPlayerToOpenPlanning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (servicePlanningUI == null)
            servicePlanningUI = FindFirstObjectByType<ServicePlanningUI>(FindObjectsInactive.Include);

        if (barberWorkController == null)
            barberWorkController = FindFirstObjectByType<BarberWorkController>();

        if (advancedWorkflow == null)
            advancedWorkflow = FindFirstObjectByType<AdvancedServiceWorkflowManager>();

        TryFindPlayerTransform();
    }

    public bool HasClientInService()
    {
        return currentClient != null;
    }

    public bool TryStartService(ClientNPC client)
    {
        if (client == null)
            return false;

        if (currentClient != null)
        {
            Debug.LogWarning("[BarbershopServiceManager] Já existe um cliente em atendimento.");
            return false;
        }

        if (client.RequestData == null)
        {
            Debug.LogWarning($"[BarbershopServiceManager] Cliente {client.name} não possui pedido configurado.");
            return false;
        }

        if (PlayerEnergySystem.Instance != null && !PlayerEnergySystem.Instance.CanStartService())
        {
            Debug.LogWarning("[BarbershopServiceManager] Energia insuficiente para iniciar atendimento.");
            return false;
        }

        if (barberChairWalkPoint == null)
        {
            Debug.LogWarning("[BarbershopServiceManager] barberChairWalkPoint não configurado.");
            return false;
        }

        ClientRequestData request = client.RequestData;

        PreparedServiceLoadout loadout = client.PreparedLoadout;

        if (loadout == null)
            loadout = new PreparedServiceLoadout();

        client.SetPreparedLoadout(loadout);

        currentClient = client;
        currentServiceUsingAdvancedWorkflow = false;
        waitingForPlayerToOpenPlanning = false;

        if (BarberQueueSystem.Instance != null)
            BarberQueueSystem.Instance.MarkClientAsBeingServed(client, true);

        float equipmentQuality = CalculateEquipmentQuality(loadout);
        float productQuality = CalculateProductQuality(loadout);
        float expectedDuration = GetExpectedServiceDuration(request);

        if (barberWorkController != null)
        {
            barberWorkController.SetCurrentServiceInfo(request.RequestName, request.ServicePrice);
            barberWorkController.StartService(
                client,
                request,
                expectedDuration,
                equipmentQuality,
                productQuality,
                defaultEnvironmentComfortScore
            );
        }

        if (enableDebugLogs)
        {
            Debug.Log(
                $"[BarbershopServiceManager] Cliente aceito e enviado para cadeira | " +
                $"Cliente: {client.name} | Pedido: {request.RequestName}"
            );
        }

        client.StartService(barberChairWalkPoint, barberChairSitPoint);

        if (currentServiceRoutine != null)
            StopCoroutine(currentServiceRoutine);

        currentServiceRoutine = StartCoroutine(WaitClientSitThenStartServiceFlow(client, expectedDuration));

        return true;
    }

    private IEnumerator WaitClientSitThenStartServiceFlow(ClientNPC client, float expectedDuration)
    {
        if (client == null)
            yield break;

        float timeout = 20f;
        float elapsed = 0f;

        while (client != null && client.CurrentState != ClientNPC.ClientState.InService && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (client == null || currentClient != client)
            yield break;

        if (client.CurrentState != ClientNPC.ClientState.InService)
            Debug.LogWarning("[BarbershopServiceManager] Cliente não chegou ao estado InService dentro do tempo esperado.");

        if (useAdvancedServiceWorkflow && openPlanningUIBeforeAdvancedExecution)
        {
            bool openedPlanning = TryOpenServicePlanningUI(client);

            if (openedPlanning)
            {
                if (enableDebugLogs)
                    Debug.Log("[BarbershopServiceManager] UI de planejamento aberta. Aguardando jogador iniciar atendimento ou dispensar.");

                currentServiceRoutine = null;
                yield break;
            }

            if (CanWaitForPlayerToOpenPlanning(client))
            {
                waitingForPlayerToOpenPlanning = true;

                if (enableDebugLogs)
                    Debug.Log("[BarbershopServiceManager] Cliente sentado. Aguardando player se aproximar da cadeira para abrir planejamento.");

                currentServiceRoutine = null;
                yield break;
            }
        }

        bool advancedStarted = TryStartAdvancedServiceFlow(client);

        if (advancedStarted)
        {
            if (enableDebugLogs)
                Debug.Log("[BarbershopServiceManager] Atendimento avançado iniciado. Timer antigo não será usado.");

            currentServiceRoutine = null;
            yield break;
        }

        if (!fallbackToOldAutoServiceIfAdvancedFails)
        {
            Debug.LogWarning("[BarbershopServiceManager] Atendimento avançado falhou e fallback antigo está desligado.");
            currentServiceRoutine = null;
            yield break;
        }

        if (autoCompleteServiceByTime)
        {
            if (enableDebugLogs)
                Debug.Log("[BarbershopServiceManager] Usando atendimento antigo automático como fallback.");

            yield return AutoCompleteServiceRoutine(client, expectedDuration);
        }

        currentServiceRoutine = null;
    }

    private bool CanWaitForPlayerToOpenPlanning(ClientNPC client)
    {
        if (!useAdvancedServiceWorkflow)
            return false;

        if (!openPlanningUIBeforeAdvancedExecution)
            return false;

        if (client == null || client.RequestData == null)
            return false;

        if (client.CurrentState != ClientNPC.ClientState.InService)
            return false;

        if (advancedWorkflow == null)
            advancedWorkflow = FindFirstObjectByType<AdvancedServiceWorkflowManager>();

        if (advancedWorkflow == null || !advancedWorkflow.EnableAdvancedWorkflow)
            return false;

        if (servicePlanningUI == null)
            servicePlanningUI = FindFirstObjectByType<ServicePlanningUI>(FindObjectsInactive.Include);

        return servicePlanningUI != null;
    }

    public void TryOpenPlanningForCurrentClient()
    {
        if (currentClient == null)
        {
            Debug.LogWarning("[BarbershopServiceManager] Nenhum cliente atual para abrir planejamento.");
            return;
        }

        bool opened = TryOpenServicePlanningUI(currentClient);

        if (!opened)
            Debug.LogWarning("[BarbershopServiceManager] Não foi possível abrir o planejamento. Verifique se o player está perto da cadeira e se o cliente está sentado.");
    }

    public bool CanOpenPlanningForCurrentClient()
    {
        if (currentClient == null)
            return false;

        if (currentClient.CurrentState != ClientNPC.ClientState.InService)
            return false;

        if (requirePlayerNearChairToOpenPlanning && !IsPlayerNearPlanningPoint())
            return false;

        return true;
    }

    private bool TryOpenServicePlanningUI(ClientNPC client)
    {
        if (!useAdvancedServiceWorkflow)
            return false;

        if (client == null || client.RequestData == null)
            return false;

        if (client.CurrentState != ClientNPC.ClientState.InService)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[BarbershopServiceManager] Planejamento bloqueado: cliente ainda não está sentado na cadeira.");

            return false;
        }

        if (requirePlayerNearChairToOpenPlanning && !IsPlayerNearPlanningPoint())
        {
            if (enableDebugLogs)
                Debug.LogWarning("[BarbershopServiceManager] Planejamento bloqueado: player está longe da cadeira de barbeiro.");

            return false;
        }

        if (advancedWorkflow == null)
            advancedWorkflow = FindFirstObjectByType<AdvancedServiceWorkflowManager>();

        if (advancedWorkflow == null || !advancedWorkflow.EnableAdvancedWorkflow)
            return false;

        if (servicePlanningUI == null)
            servicePlanningUI = FindFirstObjectByType<ServicePlanningUI>(FindObjectsInactive.Include);

        if (servicePlanningUI == null)
        {
            Debug.LogWarning("[BarbershopServiceManager] ServicePlanningUI não encontrada.");
            return false;
        }

        waitingForPlayerToOpenPlanning = false;

        servicePlanningUI.Open(client, StartAdvancedServiceFromPlanningUI);
        return true;
    }

    private bool IsPlayerNearPlanningPoint()
    {
        if (playerTransform == null)
            TryFindPlayerTransform();

        if (playerTransform == null)
        {
            Debug.LogWarning("[BarbershopServiceManager] PlayerTransform não encontrado. Configure manualmente ou use a tag Player.");
            return false;
        }

        Transform referencePoint = planningInteractionPoint;

        if (referencePoint == null)
            referencePoint = barberChairSitPoint != null ? barberChairSitPoint : barberChairWalkPoint;

        if (referencePoint == null)
        {
            Debug.LogWarning("[BarbershopServiceManager] Nenhum ponto de referência da cadeira foi configurado para o planejamento.");
            return false;
        }

        float distance = Vector3.Distance(playerTransform.position, referencePoint.position);
        return distance <= planningInteractionDistance;
    }

    private void TryFindPlayerTransform()
    {
        if (playerTransform != null)
            return;

        if (string.IsNullOrWhiteSpace(playerTag))
            return;

        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
            playerTransform = playerObject.transform;
    }

    private bool TryStartAdvancedServiceFlow(ClientNPC client)
    {
        if (!useAdvancedServiceWorkflow)
            return false;

        if (advancedWorkflow == null)
            advancedWorkflow = FindFirstObjectByType<AdvancedServiceWorkflowManager>();

        if (advancedWorkflow == null || !advancedWorkflow.EnableAdvancedWorkflow)
            return false;

        if (client == null || client.RequestData == null)
            return false;

        bool planPrepared = advancedWorkflow.TryPreparePlan(client);

        if (!planPrepared)
        {
            Debug.LogWarning("[Atendimento Avançado] TryPreparePlan retornou false.");
            return false;
        }

        bool executionStarted = advancedWorkflow.TryExecutePlan(client, result =>
        {
            HandleAdvancedServiceFinished(client, result);
        });

        if (!executionStarted)
        {
            Debug.LogWarning("[Atendimento Avançado] TryExecutePlan retornou false.");
            return false;
        }

        currentServiceUsingAdvancedWorkflow = true;
        return true;
    }

    private void StartAdvancedServiceFromPlanningUI(ClientNPC client)
    {
        if (client == null)
            return;

        if (currentClient != client)
        {
            Debug.LogWarning("[BarbershopServiceManager] Tentou iniciar plano de um cliente que não é o atendimento atual.");
            return;
        }

        bool executionStarted = TryExecutePreparedAdvancedPlan(client);

        if (!executionStarted)
        {
            Debug.LogWarning("[BarbershopServiceManager] Plano manual não executou. Tentando fluxo automático avançado.");

            bool automaticAdvancedStarted = TryStartAdvancedServiceFlow(client);

            if (!automaticAdvancedStarted && fallbackToOldAutoServiceIfAdvancedFails)
            {
                float expectedDuration = GetExpectedServiceDuration(client.RequestData);
                currentServiceRoutine = StartCoroutine(AutoCompleteServiceRoutine(client, expectedDuration));
            }
        }
    }

    private bool TryExecutePreparedAdvancedPlan(ClientNPC client)
    {
        if (advancedWorkflow == null)
            advancedWorkflow = FindFirstObjectByType<AdvancedServiceWorkflowManager>();

        if (advancedWorkflow == null)
            return false;

        if (!advancedWorkflow.HasValidPlan(client))
        {
            Debug.LogWarning("[BarbershopServiceManager] Nenhum plano manual válido encontrado para este cliente.");
            return false;
        }

        bool executionStarted = advancedWorkflow.TryExecutePlan(client, result =>
        {
            HandleAdvancedServiceFinished(client, result);
        });

        if (executionStarted)
        {
            currentServiceUsingAdvancedWorkflow = true;

            if (enableDebugLogs)
                Debug.Log("[BarbershopServiceManager] Atendimento avançado manual iniciado.");
        }

        return executionStarted;
    }

    private void HandleAdvancedServiceFinished(ClientNPC client, AdvancedServiceResult result)
    {
        if (client == null || currentClient != client)
            return;

        if (currentServiceRoutine != null)
        {
            StopCoroutine(currentServiceRoutine);
            currentServiceRoutine = null;
        }

        AdvancedServiceExecutionUI executionUI =
            FindFirstObjectByType<AdvancedServiceExecutionUI>(FindObjectsInactive.Include);

        if (executionUI != null)
        {
            executionUI.HideAfterDelay(advancedExecutionUiCloseDelay, () =>
            {
                ShowAdvancedEvaluationThenFinish(client, result);
            });
        }
        else
        {
            ShowAdvancedEvaluationThenFinish(client, result);
        }
    }

    private void ShowAdvancedEvaluationThenFinish(ClientNPC client, AdvancedServiceResult result)
    {
        if (client == null || currentClient != client)
            return;

        ServiceEvaluationUI evaluationUI =
            FindFirstObjectByType<ServiceEvaluationUI>(FindObjectsInactive.Include);

        if (evaluationUI != null && result != null)
            evaluationUI.ShowAdvanced(result);

        if (completeClientVisualFlowAfterAdvancedService)
            CompleteCurrentServiceAfterAdvancedReward();
        else
            SendCurrentClientToExitOrCashier();
    }

    private void CompleteCurrentServiceAfterAdvancedReward()
    {
        if (currentClient == null)
            return;

        ClientNPC finishedClient = currentClient;
        ClientRequestData request = finishedClient.RequestData;

        if (request == null)
        {
            SendCurrentClientToExitOrCashier();
            return;
        }

        if (currentServiceRoutine != null)
        {
            StopCoroutine(currentServiceRoutine);
            currentServiceRoutine = null;
        }

        PreparedServiceLoadout loadout = finishedClient.PreparedLoadout;

        if (consumeInventoryOnFinish)
            ConsumeLoadoutItems(request, loadout);

        finishedClient.MarkServiceCompleted();
        UnlockEducationalContent(request);

        SendCurrentClientToExitOrCashier();
    }

    public void DismissCurrentClientFromPlanning(ClientNPC client)
    {
        if (client == null)
            return;

        if (currentClient != client)
            return;

        if (currentServiceRoutine != null)
        {
            StopCoroutine(currentServiceRoutine);
            currentServiceRoutine = null;
        }

        currentServiceUsingAdvancedWorkflow = false;
        waitingForPlayerToOpenPlanning = false;

        if (BarberQueueSystem.Instance != null)
            BarberQueueSystem.Instance.RemoveClientFromQueue(client);

        currentClient = null;

        if (exitPoint != null)
            client.LeaveShop(exitPoint);
        else
            client.ForceDespawn();

        if (enableDebugLogs)
            Debug.Log($"[BarbershopServiceManager] Cliente {client.name} foi dispensado pelo jogador.");
    }

    private IEnumerator AutoCompleteServiceRoutine(ClientNPC client, float expectedDurationMinutes)
    {
        if (client == null)
            yield break;

        float duration = expectedDurationMinutes;

        if (barberWorkController != null)
            duration = barberWorkController.GetAdjustedServiceDuration(expectedDurationMinutes);

        float seconds = Mathf.Max(1f, duration);

        yield return new WaitForSeconds(seconds);

        if (currentClient == client && !currentServiceUsingAdvancedWorkflow)
            CompleteCurrentService();
    }

    public void CompleteCurrentService()
    {
        if (currentClient == null || currentServiceUsingAdvancedWorkflow)
            return;

        ClientNPC finishedClient = currentClient;
        ClientRequestData request = finishedClient.RequestData;

        if (request == null)
        {
            SendCurrentClientToExitOrCashier();
            return;
        }

        if (currentServiceRoutine != null)
        {
            StopCoroutine(currentServiceRoutine);
            currentServiceRoutine = null;
        }

        PreparedServiceLoadout loadout = finishedClient.PreparedLoadout;

        float equipmentQuality = CalculateEquipmentQuality(loadout);
        float productQuality = CalculateProductQuality(loadout);
        float expectedDuration = GetExpectedServiceDuration(request);
        float actualDuration = GetActualServiceDuration(expectedDuration);

        if (consumeInventoryOnFinish)
            ConsumeLoadoutItems(request, loadout);

        ServiceEvaluationResult evaluationResult = null;

        if (barberWorkController != null)
        {
            evaluationResult = barberWorkController.FinishService(
                actualDuration,
                equipmentQuality,
                productQuality,
                defaultHadMistakes
            );
        }

        finishedClient.MarkServiceCompleted();

        UnlockEducationalContent(request);
        AddServicePayment(request, evaluationResult);
        AddServiceXP(request);
        AddServiceRating(evaluationResult, equipmentQuality, productQuality);

        if (MissionSystem.Instance != null)
        {
            MissionSystem.Instance.RegisterServiceCompleted(
                request,
                Mathf.Max(0, request.ServicePrice),
                Mathf.Max(0f, actualDuration)
            );
        }

        SendCurrentClientToExitOrCashier();
    }

    private void SendCurrentClientToExitOrCashier()
    {
        if (currentClient == null)
            return;

        currentServiceUsingAdvancedWorkflow = false;
        waitingForPlayerToOpenPlanning = false;

        if (cashierPoint != null)
        {
            currentClient.GoToCashier(cashierPoint);
        }
        else if (exitPoint != null)
        {
            ClientNPC client = currentClient;
            currentClient = null;
            client.LeaveShop(exitPoint);
        }
        else
        {
            ClientNPC client = currentClient;
            currentClient = null;
            client.ForceDespawn();
        }
    }

    private float GetExpectedServiceDuration(ClientRequestData request)
    {
        return request == null ? 1f : Mathf.Max(1f, request.ServiceTime);
    }

    private float GetActualServiceDuration(float expectedDuration)
    {
        if (barberWorkController != null)
            return barberWorkController.GetAdjustedServiceDuration(expectedDuration);

        if (PlayerEnergySystem.Instance != null)
            return expectedDuration * PlayerEnergySystem.Instance.GetServiceTimeMultiplier();

        return expectedDuration;
    }

    private void ConsumeLoadoutItems(ClientRequestData request, PreparedServiceLoadout loadout)
    {
        if (request == null || request.requiredItems == null || request.requiredItems.Count == 0)
            return;

        if (loadout == null || InventoryManager.Instance == null)
            return;

        foreach (ServiceRequirementData requirement in request.requiredItems)
        {
            if (requirement == null)
                continue;

            PreparedServiceItemSelection selection = loadout.GetSelectionByRequirement(requirement.requirementId);

            if (selection == null)
                continue;

            switch (requirement.usageType)
            {
                case InventoryUsageType.PorHoraDeUso:
                    InventoryManager.Instance.ConsumeDurableHoursByUniqueId(
                        selection.productUniqueId,
                        Mathf.Max(1, requirement.hoursConsumed)
                    );
                    break;

                case InventoryUsageType.PorServico:
                default:
                    InventoryManager.Instance.ConsumeProductUsageByUniqueId(
                        selection.productUniqueId,
                        Mathf.Max(1, requirement.amountConsumed)
                    );
                    break;
            }
        }

        InventoryManager.Instance.RemoveAllUnusableItems();
    }

    private float CalculateEquipmentQuality(PreparedServiceLoadout loadout)
    {
        return CalculateAverageQuality(loadout, false);
    }

    private float CalculateProductQuality(PreparedServiceLoadout loadout)
    {
        return CalculateAverageQuality(loadout, true);
    }

    private float CalculateAverageQuality(PreparedServiceLoadout loadout, bool includeConsumables)
    {
        if (loadout == null || loadout.selections == null || loadout.selections.Count == 0)
            return 3f;

        if (InventoryManager.Instance == null)
            return 3f;

        float total = 0f;
        int count = 0;

        foreach (PreparedServiceItemSelection selection in loadout.selections)
        {
            if (selection == null)
                continue;

            ProductData product = InventoryManager.Instance.GetProductDataById(selection.productId);

            if (product == null)
                continue;

            bool isConsumable = product.inventoryItemType != InventoryItemType.Duravel;

            if (includeConsumables != isConsumable)
                continue;

            total += ProductToFiveStarScore(product);
            count++;
        }

        return count <= 0 ? 3f : Mathf.Clamp(total / count, 0f, 5f);
    }

    private float ProductToFiveStarScore(ProductData product)
    {
        if (product == null)
            return 3f;

        float precision = Mathf.Clamp(product.precisao, 0f, 100f);
        float speed = Mathf.Clamp(product.velocidade, 0f, 100f);
        float durability = Mathf.Clamp(product.durabilidade, 0f, 100f);

        return Mathf.Clamp(((precision + speed + durability) / 3f) / 20f, 0f, 5f);
    }

    private void UnlockEducationalContent(ClientRequestData request)
    {
        if (request == null || EducationProgressManager.Instance == null)
            return;

        if (!string.IsNullOrWhiteSpace(request.afroCutId))
            EducationProgressManager.Instance.UnlockCut(request.afroCutId);
    }

    private void AddServicePayment(ClientRequestData request, ServiceEvaluationResult evaluationResult)
    {
        if (request == null)
            return;

        if (FinanceManager.Instance != null)
            FinanceManager.Instance.RegisterServiceIncome(request, currentClient != null ? currentClient.name : "");

        if (ClientSpawnerLocator.TryGet(out ClientSpawner spawner))
        {
            int finalPrice = request.ServicePrice;
            int suggestedPrice = GlobalGameplayManagement.Instance != null
                ? GlobalGameplayManagement.Instance.CalculateSuggestedPriceForRequest(request)
                : finalPrice;

            spawner.UpdateDemandMultiplierFromServicePrice(finalPrice, suggestedPrice);
        }
    }

    public void NotifyClientFinishedCashier(ClientNPC client)
    {
        if (client == null)
            return;

        if (client == currentClient)
        {
            currentClient = null;
            currentServiceUsingAdvancedWorkflow = false;
            waitingForPlayerToOpenPlanning = false;
        }

        if (BarberQueueSystem.Instance != null)
            BarberQueueSystem.Instance.RemoveClientFromQueue(client);

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
            currentServiceUsingAdvancedWorkflow = false;
            waitingForPlayerToOpenPlanning = false;

            if (currentServiceRoutine != null)
            {
                StopCoroutine(currentServiceRoutine);
                currentServiceRoutine = null;
            }
        }

        if (BarberQueueSystem.Instance != null)
            BarberQueueSystem.Instance.RemoveClientFromQueue(client);
    }

    private void AddServiceXP(ClientRequestData request)
    {
        if (request == null || PlayerXPManager.Instance == null)
            return;

        PlayerXPManager.Instance.AddXP(Mathf.Max(0, request.xpReward));
    }

    private void AddServiceRating(ServiceEvaluationResult evaluationResult, float equipmentQuality, float productQuality)
    {
        if (BarbershopRatingManager.Instance == null)
            return;

        float rating = evaluationResult != null
            ? evaluationResult.finalScore
            : (equipmentQuality + productQuality + defaultEnvironmentComfortScore) / 3f;

        BarbershopRatingManager.Instance.AddReview(rating);
    }

    public bool CallNextClientFromQueue()
    {
        if (BarberQueueSystem.Instance != null &&
            BarberQueueSystem.Instance.TryGetNextWaitingClient(out ClientNPC queuedClient) &&
            queuedClient != null)
        {
            queuedClient.CallForService();
            return true;
        }

        ClientNPC[] clients = FindObjectsByType<ClientNPC>(FindObjectsSortMode.None);

        for (int i = 0; i < clients.Length; i++)
        {
            ClientNPC client = clients[i];

            if (client == null || !client.IsWaitingForService)
                continue;

            client.CallForService();
            return true;
        }

        Debug.LogWarning("[BarbershopServiceManager] Nenhum cliente em WaitingForService encontrado.");
        return false;
    }
}