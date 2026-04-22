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
    [SerializeField] private bool sendClientAwayIfMissingItems = false;
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

        if (loadout == null || !ServiceLoadoutBuilder.IsLoadoutComplete(request, loadout))
        {
            loadout = PrepareLoadoutForClient(request);
        }

        if (!ServiceLoadoutBuilder.IsLoadoutComplete(request, loadout))
        {
            Debug.LogWarning($"[BarbershopServiceManager] Itens insuficientes para atender: {request.RequestName}");

            if (sendClientAwayIfMissingItems)
                client.DispenseDueToMissingItems();

            return false;
        }

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
                expectedDuration,
                equipmentQuality,
                productQuality,
                defaultEnvironmentComfortScore
            );
        }

        if (enableDebugLogs)
            Debug.Log($"[BarbershopServiceManager] Iniciando atendimento de {client.name} | Pedido: {request.RequestName}");

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
        {
            Debug.LogWarning("[BarbershopServiceManager] Cliente não chegou ao estado InService dentro do tempo esperado.");
        }

        if (useAdvancedServiceWorkflow && openPlanningUIBeforeAdvancedExecution)
        {
            bool openedPlanning = TryOpenServicePlanningUI(client);

            if (openedPlanning)
            {
                if (enableDebugLogs)
                    Debug.Log("[BarbershopServiceManager] UI de planejamento aberta. Aguardando jogador iniciar atendimento.");

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
        {
            Debug.LogWarning("[BarbershopServiceManager] Não foi possível abrir o planejamento. Verifique se o player está perto da cadeira e se o cliente está sentado.");
        }
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
        {
            if (enableDebugLogs)
                Debug.Log("[Atendimento Avançado] useAdvancedServiceWorkflow está desligado no BarbershopServiceManager.");

            return false;
        }

        if (advancedWorkflow == null)
        {
            advancedWorkflow = FindFirstObjectByType<AdvancedServiceWorkflowManager>();
        }

        if (advancedWorkflow == null)
        {
            Debug.LogWarning("[Atendimento Avançado] AdvancedServiceWorkflowManager não encontrado na cena.");
            return false;
        }

        if (!advancedWorkflow.EnableAdvancedWorkflow)
        {
            Debug.LogWarning("[Atendimento Avançado] EnableAdvancedWorkflow está desligado no AdvancedServiceWorkflowManager.");
            return false;
        }

        if (client == null)
        {
            Debug.LogWarning("[Atendimento Avançado] Cliente nulo.");
            return false;
        }

        if (client.RequestData == null)
        {
            Debug.LogWarning("[Atendimento Avançado] ClientNPC.RequestData está nulo.");
            return false;
        }

        if (enableDebugLogs)
            Debug.Log("[Atendimento Avançado] Tentando preparar plano...");

        bool planPrepared = advancedWorkflow.TryPreparePlan(client);

        if (!planPrepared)
        {
            Debug.LogWarning("[Atendimento Avançado] TryPreparePlan retornou false.");
            return false;
        }

        if (enableDebugLogs)
            Debug.Log("[Atendimento Avançado] Plano preparado. Tentando executar...");

        bool executionStarted = advancedWorkflow.TryExecutePlan(client, result =>
        {
            if (enableDebugLogs)
            {
                string rating = result != null ? result.finalRating.ToString() : "NULL";
                Debug.Log($"[Atendimento Avançado] Atendimento finalizado. Resultado: {rating}");
            }

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
            if (enableDebugLogs)
            {
                string rating = result != null ? result.finalRating.ToString() : "NULL";
                Debug.Log($"[Atendimento Avançado] Atendimento finalizado. Resultado: {rating}");
            }

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
        if (client == null)
            return;

        if (currentClient != client)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[BarbershopServiceManager] Callback do avançado recebido, mas este cliente não é mais o currentClient.");

            return;
        }

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
            Debug.LogWarning("[BarbershopServiceManager] AdvancedServiceExecutionUI não encontrada. Exibindo avaliação imediatamente.");
            ShowAdvancedEvaluationThenFinish(client, result);
        }
    }

    private void ShowAdvancedEvaluationThenFinish(ClientNPC client, AdvancedServiceResult result)
    {
        if (client == null)
            return;

        if (currentClient != client)
            return;

        ServiceEvaluationUI evaluationUI =
            FindFirstObjectByType<ServiceEvaluationUI>(FindObjectsInactive.Include);

        if (evaluationUI != null && result != null)
        {
            evaluationUI.ShowAdvanced(result);
        }
        else
        {
            Debug.LogWarning("[BarbershopServiceManager] ServiceEvaluationUI não encontrada ou resultado avançado nulo.");
        }

        if (completeClientVisualFlowAfterAdvancedService)
        {
            CompleteCurrentServiceAfterAdvancedReward();
        }
        else
        {
            SendCurrentClientToExitOrCashier();
        }
    }

    private void CompleteCurrentServiceAfterAdvancedReward()
    {
        if (currentClient == null)
        {
            Debug.LogWarning("[BarbershopServiceManager] Não existe cliente atual para concluir fluxo avançado.");
            return;
        }

        ClientNPC finishedClient = currentClient;
        ClientRequestData request = finishedClient.RequestData;

        if (request == null)
        {
            Debug.LogWarning("[BarbershopServiceManager] Cliente atual não possui RequestData.");
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

        if (enableDebugLogs)
        {
            Debug.Log(
                $"[BarbershopServiceManager] Fluxo avançado concluído | Cliente: {finishedClient.name} | " +
                $"Pedido: {request.RequestName}. Recompensa/XP já foram aplicados pelo AdvancedServiceWorkflowManager."
            );
        }

        SendCurrentClientToExitOrCashier();
    }

    public void DismissCurrentClientFromPlanning(ClientNPC client)
    {
        if (client == null)
            return;

        if (currentClient != client)
        {
            Debug.LogWarning("[BarbershopServiceManager] Tentou dispensar um cliente que não é o cliente atual.");
            return;
        }

        if (currentServiceRoutine != null)
        {
            StopCoroutine(currentServiceRoutine);
            currentServiceRoutine = null;
        }

        currentServiceUsingAdvancedWorkflow = false;
        waitingForPlayerToOpenPlanning = false;

        if (BarberQueueSystem.Instance != null)
        {
            BarberQueueSystem.Instance.RemoveClientFromQueue(client);
        }

        currentClient = null;

        if (exitPoint != null)
        {
            client.LeaveShop(exitPoint);
        }
        else
        {
            client.ForceDespawn();
        }

        if (enableDebugLogs)
        {
            Debug.Log($"[BarbershopServiceManager] Cliente {client.name} foi dispensado pelo jogador.");
        }
    }

    private PreparedServiceLoadout PrepareLoadoutForClient(ClientRequestData request)
    {
        if (request == null)
            return new PreparedServiceLoadout();

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[BarbershopServiceManager] InventoryManager.Instance não encontrado.");
            return new PreparedServiceLoadout();
        }

        return ServiceLoadoutBuilder.BuildDefaultLoadout(request);
    }

    private IEnumerator AutoCompleteServiceRoutine(ClientNPC client, float expectedDurationMinutes)
    {
        if (client == null)
            yield break;

        float duration = expectedDurationMinutes;

        if (barberWorkController != null)
            duration = barberWorkController.GetAdjustedServiceDuration(expectedDurationMinutes);

        float seconds = ConvertGameMinutesToRealSeconds(duration);

        if (enableDebugLogs)
            Debug.Log($"[BarbershopServiceManager] Atendimento automático antigo durará {seconds:0.0}s reais.");

        yield return new WaitForSeconds(seconds);

        if (currentClient == client && !currentServiceUsingAdvancedWorkflow)
            CompleteCurrentService();
    }

    private float ConvertGameMinutesToRealSeconds(float gameMinutes)
    {
        return Mathf.Max(1f, gameMinutes);
    }

    public void CompleteCurrentService()
    {
        if (currentClient == null)
        {
            Debug.LogWarning("[BarbershopServiceManager] Não existe cliente atual para concluir atendimento.");
            return;
        }

        if (currentServiceUsingAdvancedWorkflow)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[BarbershopServiceManager] CompleteCurrentService chamado enquanto atendimento avançado está ativo. Ignorando para evitar conclusão dupla.");

            return;
        }

        ClientNPC finishedClient = currentClient;
        ClientRequestData request = finishedClient.RequestData;

        if (request == null)
        {
            Debug.LogWarning("[BarbershopServiceManager] Cliente atual não possui RequestData.");
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

        if (enableDebugLogs)
        {
            Debug.Log(
                $"[BarbershopServiceManager] Atendimento antigo concluído | Cliente: {finishedClient.name} | " +
                $"Pedido: {request.RequestName} | Valor: {request.ServicePrice}"
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
        if (request == null)
            return 1f;

        return Mathf.Max(1f, request.ServiceTime);
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

        if (loadout == null)
        {
            Debug.LogWarning("[BarbershopServiceManager] Loadout nulo. Não foi possível consumir itens.");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[BarbershopServiceManager] InventoryManager.Instance não encontrado. Itens não consumidos.");
            return;
        }

        foreach (ServiceRequirementData requirement in request.requiredItems)
        {
            if (requirement == null)
                continue;

            PreparedServiceItemSelection selection = loadout.GetSelectionByRequirement(requirement.requirementId);

            if (selection == null)
            {
                Debug.LogWarning($"[BarbershopServiceManager] Nenhuma seleção encontrada para requisito: {requirement.GetDisplayName()}");
                continue;
            }

            bool consumed = false;

            switch (requirement.usageType)
            {
                case InventoryUsageType.PorHoraDeUso:
                    consumed = InventoryManager.Instance.ConsumeDurableHoursByUniqueId(
                        selection.productUniqueId,
                        Mathf.Max(1, requirement.hoursConsumed)
                    );
                    break;

                case InventoryUsageType.PorServico:
                default:
                    consumed = InventoryManager.Instance.ConsumeProductUsageByUniqueId(
                        selection.productUniqueId,
                        Mathf.Max(1, requirement.amountConsumed)
                    );
                    break;
            }

            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[BarbershopServiceManager] Consumo de item | Requisito: {requirement.GetDisplayName()} | " +
                    $"Produto: {selection.productId} | Sucesso: {consumed}"
                );
            }
        }

        InventoryManager.Instance.RemoveAllUnusableItems();
    }

    private float CalculateEquipmentQuality(PreparedServiceLoadout loadout)
    {
        return CalculateAverageQuality(loadout, includeConsumables: false);
    }

    private float CalculateProductQuality(PreparedServiceLoadout loadout)
    {
        return CalculateAverageQuality(loadout, includeConsumables: true);
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

            float score = ProductToFiveStarScore(product);
            total += score;
            count++;
        }

        if (count <= 0)
            return 3f;

        return Mathf.Clamp(total / count, 0f, 5f);
    }

    private float ProductToFiveStarScore(ProductData product)
    {
        if (product == null)
            return 3f;

        float precision = Mathf.Clamp(product.precisao, 0f, 100f);
        float speed = Mathf.Clamp(product.velocidade, 0f, 100f);
        float durability = Mathf.Clamp(product.durabilidade, 0f, 100f);

        float average = (precision + speed + durability) / 3f;

        return Mathf.Clamp(average / 20f, 0f, 5f);
    }

    private void UnlockEducationalContent(ClientRequestData request)
    {
        if (request == null)
            return;

        if (EducationProgressManager.Instance == null)
            return;

        if (string.IsNullOrWhiteSpace(request.afroCutId))
            return;

        EducationProgressManager.Instance.UnlockCut(request.afroCutId);
    }

    private void AddServicePayment(ClientRequestData request, ServiceEvaluationResult evaluationResult)
    {
        if (request == null)
            return;

        if (FinanceManager.Instance != null)
        {
            FinanceManager.Instance.RegisterServiceIncome(
                request,
                currentClient != null ? currentClient.name : ""
            );
        }
        else
        {
            Debug.LogWarning("[BarbershopServiceManager] FinanceManager.Instance não encontrado. Dinheiro não foi adicionado ao caixa.");
        }

        Debug.Log($"[BarbershopServiceManager] Pagamento recebido: R$ {request.ServicePrice} | Serviço: {request.RequestName}");
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

            if (enableDebugLogs)
                Debug.Log($"[BarbershopServiceManager] Cliente {client.name} terminou no caixa.");
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

            if (enableDebugLogs)
                Debug.Log($"[BarbershopServiceManager] Cliente {client.name} liberado do atendimento.");
        }

        if (BarberQueueSystem.Instance != null)
            BarberQueueSystem.Instance.RemoveClientFromQueue(client);
    }

    private void AddServiceXP(ClientRequestData request)
    {
        if (request == null)
            return;

        if (PlayerXPManager.Instance == null)
        {
            Debug.LogWarning("[BarbershopServiceManager] PlayerXPManager.Instance não encontrado. XP não foi adicionado.");
            return;
        }

        PlayerXPManager.Instance.AddXP(Mathf.Max(0, request.xpReward));
    }

    private void AddServiceRating(ServiceEvaluationResult evaluationResult, float equipmentQuality, float productQuality)
    {
        if (BarbershopRatingManager.Instance == null)
        {
            Debug.LogWarning("[BarbershopServiceManager] BarbershopRatingManager.Instance não encontrado. Avaliação não foi registrada.");
            return;
        }

        float rating = 3f;

        if (evaluationResult != null)
        {
            rating = evaluationResult.finalScore;
        }
        else
        {
            rating = (equipmentQuality + productQuality + defaultEnvironmentComfortScore) / 3f;
        }

        BarbershopRatingManager.Instance.AddReview(rating);
    }

    public bool CallNextClientFromQueue()
    {
        ClientNPC[] clients = FindObjectsOfType<ClientNPC>();

        foreach (ClientNPC client in clients)
        {
            if (client == null)
                continue;

            if (!client.IsWaitingForService)
                continue;

            Debug.Log("[BarbershopServiceManager] Chamando próximo cliente da fila: " + client.name);

            client.CallForService();

            return true;
        }

        Debug.LogWarning("[BarbershopServiceManager] Nenhum cliente em WaitingForService encontrado.");
        return false;
    }
}