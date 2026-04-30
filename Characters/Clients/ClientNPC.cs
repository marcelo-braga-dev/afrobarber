using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ClientNPC : MonoBehaviour
{
    public enum ClientState
    {
        None,
        GoingToEntrance,
        GoingToWaitingPoint,
        WaitingForService,
        GoingToBarberChair,
        InService,
        GoingToCashier,
        Leaving
    }

    [Header("Referências")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject interactionIcon;
    [SerializeField] private ClientHairVisualController hairVisualController;

    [Header("Perfil fixo do cliente")]
    [SerializeField] private ClientServiceProfile serviceProfile;

    [Header("Configuração")]
    [SerializeField] private string clientDisplayName = "Cliente";
    [SerializeField] private float arrivalDistance = 0.85f;
    [SerializeField] private float cashierWaitTime = 2f;
    [SerializeField] private float maxPatienceMinutes = 90f;

    [Header("Anti-travamento / NavMesh")]
    [SerializeField] private float navMeshSampleRadius = 2f;
    [SerializeField] private float stuckCheckInterval = 0.5f;
    [SerializeField] private float stuckVelocityThreshold = 0.05f;
    [SerializeField] private float maxStuckTime = 3f;
    [SerializeField] private float forceArrivalDistance = 1.25f;
    [SerializeField] private bool autoAdvanceIfStuckNearTarget = true;
    [SerializeField] private bool repathWhenStuck = true;

    [Header("Ajuste de assento")]
    [SerializeField] private bool snapToSeatOnArrival = true;
    [SerializeField] private bool rotateToSeatOnArrival = true;

    [Header("Animator Params")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string sitParam = "Sit";

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private ClientSpawner spawner;
    private WaitingAreaManager waitingAreaManager;

    private Transform entrancePoint;
    private Transform barberChairWalkPoint;
    private Transform barberChairSitPoint;
    private Transform exitPoint;
    private Transform currentTarget;
    private Transform playerTransform;

    private WaitingSeat reservedSeat;
    private GameObject sourcePrefab;
    private ClientRequestData currentRequest;
    private ClientState currentState = ClientState.None;

    private bool initialized;
    private bool serviceCompleted;
    private bool finalHairApplied;
    private bool addedToQueue;

    private PreparedServiceLoadout preparedLoadout;

    private float stuckTimer;
    private float stuckCheckTimer;
    private Vector3 lastDestination;

    public string ClientDisplayName => clientDisplayName;
    public ClientRequestData CurrentRequest => currentRequest;
    public ClientRequestData RequestData => currentRequest;
    public ClientState CurrentState => currentState;
    public bool IsWaitingForService => currentState == ClientState.WaitingForService;
    public GameObject SourcePrefab => sourcePrefab;
    public ClientServiceProfile ServiceProfile => serviceProfile;
    public PreparedServiceLoadout PreparedLoadout => preparedLoadout;
    public bool ServiceCompleted => serviceCompleted;
    public float MaxPatienceMinutes => maxPatienceMinutes;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (hairVisualController == null)
            hairVisualController = GetComponentInChildren<ClientHairVisualController>(true);

        ConfigureAgent();
        HideInteractionIcon();
    }

    private void Update()
    {
        UpdateAnimator();

        if (!initialized)
            return;

        if (currentTarget == null || agent == null || !agent.enabled)
            return;

        if (HasReachedDestination())
        {
            HandleReachedDestination();
            return;
        }

        UpdateStuckDetection();
    }

    private void ConfigureAgent()
    {
        if (agent == null)
            return;

        agent.stoppingDistance = Mathf.Max(agent.stoppingDistance, 0.35f);
        agent.autoBraking = true;
        agent.updatePosition = true;
        agent.updateRotation = true;
    }

    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }

    public bool HasFixedProfileRequest()
    {
        return serviceProfile != null && serviceProfile.GetRequest() != null;
    }

    public void SetRequest(ClientRequestData request)
    {
        currentRequest = request;
        currentRequest?.SyncCompatibilityFields();
    }

    public void SetPreparedLoadout(PreparedServiceLoadout loadout)
    {
        preparedLoadout = loadout;

        if (enableDebugLogs)
            Debug.Log($"[{name}] Loadout preparado atribuído.");
    }

    public void Initialize(
        ClientSpawner ownerSpawner,
        WaitingAreaManager ownerWaitingAreaManager,
        Transform ownerEntrancePoint,
        Transform ownerBarberChairPoint,
        Transform ownerExitPoint,
        GameObject ownerSourcePrefab,
        ClientRequestData fallbackRequest = null
    )
    {
        spawner = ownerSpawner;
        waitingAreaManager = ownerWaitingAreaManager;
        entrancePoint = ownerEntrancePoint;
        barberChairWalkPoint = ownerBarberChairPoint;
        barberChairSitPoint = ownerBarberChairPoint;
        exitPoint = ownerExitPoint;
        sourcePrefab = ownerSourcePrefab;

        ResolveRequest(fallbackRequest);
        ApplyInitialHair();

        initialized = true;
        serviceCompleted = false;
        finalHairApplied = false;
        addedToQueue = false;
        preparedLoadout = null;

        WarpToNavMeshIfNeeded();

        if (enableDebugLogs)
            Debug.Log($"[{name}] Inicializado. Request atual: {(currentRequest != null ? currentRequest.RequestName : "NULL")}");

        if (entrancePoint != null)
            GoToEntrance();
        else
            GoToWaitingPoint();
    }

    private void ResolveRequest(ClientRequestData fallbackRequest)
    {
        if (HasFixedProfileRequest())
        {
            currentRequest = serviceProfile.GetRequest();
            currentRequest?.SyncCompatibilityFields();
            return;
        }

        if (fallbackRequest != null)
        {
            currentRequest = fallbackRequest;
            currentRequest?.SyncCompatibilityFields();
            return;
        }

        Debug.LogWarning($"[{name}] Nenhum request configurado para este cliente.");
    }

    private void ApplyInitialHair()
    {
        if (hairVisualController == null)
            return;

        if (currentRequest != null && !string.IsNullOrWhiteSpace(currentRequest.beforeHairId))
        {
            hairVisualController.ApplyHairById(currentRequest.beforeHairId);
            return;
        }

        if (serviceProfile != null)
            hairVisualController.ApplyBeforeHair(serviceProfile);
    }

    public void ApplyFinalHair()
    {
        if (finalHairApplied || hairVisualController == null)
            return;

        if (currentRequest != null && !string.IsNullOrWhiteSpace(currentRequest.afterHairId))
        {
            hairVisualController.ApplyHairById(currentRequest.afterHairId);
            finalHairApplied = true;
            return;
        }

        if (serviceProfile != null)
        {
            hairVisualController.ApplyAfterHair(serviceProfile);
            finalHairApplied = true;
        }
    }

    public AfroCutInfo GetCurrentCutInfo()
    {
        if (currentRequest == null || EducationProgressManager.Instance == null)
            return null;

        return EducationProgressManager.Instance.GetCutById(currentRequest.afroCutId);
    }

    public void OnPlayerClicked()
    {
        if (enableDebugLogs)
            Debug.Log($"[{name}] Clique recebido. Estado atual: {currentState}");

        if (currentState != ClientState.WaitingForService)
        {
            if (enableDebugLogs)
                Debug.LogWarning($"[{name}] Clique ignorado porque o estado não é WaitingForService.");

            return;
        }

        if (ClientRequestUI.Instance == null)
        {
            Debug.LogWarning("ClientRequestUI.Instance não encontrado.");
            return;
        }

        if (currentRequest == null)
        {
            Debug.LogWarning($"[{name}] currentRequest está nulo.");
            return;
        }

        ClientRequestUI.Instance.Show(this);
    }

    public void CallForService()
    {
        if (enableDebugLogs)
            Debug.Log($"[{name}] CallForService() chamado.");

        if (BarbershopServiceManager.Instance == null)
        {
            Debug.LogWarning($"[{name}] BarbershopServiceManager.Instance não encontrado.");
            return;
        }

        BarbershopServiceManager.Instance.TryStartService(this);
    }

    public void GoToBarberChair()
    {
        CallForService();
    }

    public void StartService(Transform walkPoint, Transform sitPoint)
    {
        barberChairWalkPoint = walkPoint;
        barberChairSitPoint = sitPoint != null ? sitPoint : walkPoint;
        BeginService(barberChairWalkPoint);
    }

    public void BeginService(Transform chairWalkPoint)
    {
        if (chairWalkPoint != null)
            barberChairWalkPoint = chairWalkPoint;

        if (barberChairWalkPoint == null)
        {
            Debug.LogWarning($"[{name}] barberChairWalkPoint não configurado.");
            return;
        }

        RemoveFromQueue();
        ReleaseReservedSeat();

        HideInteractionIcon();
        SetSit(false);

        currentTarget = barberChairWalkPoint;
        currentState = ClientState.GoingToBarberChair;
        SetDestination(barberChairWalkPoint.position);

        if (enableDebugLogs)
            Debug.Log($"[{name}] Indo para cadeira de barbeiro.");
    }

    public void DispenseDueToMissingItems()
    {
        if (enableDebugLogs)
            Debug.Log($"[{name}] Cliente dispensado por falta de itens.");

        RemoveFromQueue();
        LeaveShop(exitPoint);
    }

    public void MarkServiceCompleted()
    {
        serviceCompleted = true;
        ApplyFinalHair();

        if (enableDebugLogs)
            Debug.Log($"[{name}] Serviço marcado como concluído.");
    }

    public void GoToCashier(Transform cashierPoint)
    {
        if (cashierPoint == null)
        {
            Debug.LogWarning($"[{name}] cashierPoint não configurado.");
            LeaveShop(exitPoint);
            return;
        }

        HideInteractionIcon();
        SetSit(false);

        currentTarget = cashierPoint;
        currentState = ClientState.GoingToCashier;
        SetDestination(cashierPoint.position);
    }

    public void LeaveShop(Transform customExitPoint = null)
    {
        Transform targetExit = customExitPoint != null ? customExitPoint : exitPoint;

        if (targetExit == null)
        {
            ForceDespawn();
            return;
        }

        RemoveFromQueue();
        ReleaseReservedSeat();

        HideInteractionIcon();
        SetSit(false);

        currentTarget = targetExit;
        currentState = ClientState.Leaving;
        SetDestination(targetExit.position);
    }

    public void LeaveDueToClosingTime()
    {
        HideInteractionIcon();
        LeaveShop(exitPoint);
    }

    public void ForceDespawn()
    {
        RemoveFromQueue();
        ReleaseReservedSeat();
        NotifySpawnerFinished();
        Destroy(gameObject);
    }

    private void GoToEntrance()
    {
        HideInteractionIcon();
        SetSit(false);

        currentTarget = entrancePoint;
        currentState = ClientState.GoingToEntrance;
        SetDestination(entrancePoint.position);

        if (enableDebugLogs)
            Debug.Log($"[{name}] Indo para EntrancePoint.");
    }

    private void GoToWaitingPoint()
    {
        Transform waitingApproachPoint = GetOrReserveWaitingApproachPoint();

        if (waitingApproachPoint == null)
        {
            Debug.LogWarning($"[{name}] Nenhum assento disponível. Cliente será dispensado.");
            LeaveShop(exitPoint);
            return;
        }

        HideInteractionIcon();
        SetSit(false);

        currentTarget = waitingApproachPoint;
        currentState = ClientState.GoingToWaitingPoint;
        SetDestination(waitingApproachPoint.position);

        if (enableDebugLogs)
            Debug.Log($"[{name}] Indo para assento de espera.");
    }

    private Transform GetOrReserveWaitingApproachPoint()
    {
        if (reservedSeat != null)
            return reservedSeat.ApproachPoint;

        if (waitingAreaManager == null)
            return null;

        bool success = waitingAreaManager.TryReserveAnySeat(this, out reservedSeat);

        if (!success || reservedSeat == null)
            return null;

        return reservedSeat.ApproachPoint;
    }

    private void ReleaseReservedSeat()
    {
        if (reservedSeat == null)
            return;

        reservedSeat.Release(this);
        reservedSeat = null;
    }

    private void HandleReachedDestination()
    {
        switch (currentState)
        {
            case ClientState.GoingToEntrance:
                ArriveAtEntrance();
                break;

            case ClientState.GoingToWaitingPoint:
                ArriveAtWaitingPoint();
                break;

            case ClientState.GoingToBarberChair:
                ArriveAtBarberChairWalkPoint();
                break;

            case ClientState.GoingToCashier:
                ArriveAtCashier();
                break;

            case ClientState.Leaving:
                ArriveAtExit();
                break;
        }
    }

    private void ArriveAtEntrance()
    {
        StopAgent();

        if (enableDebugLogs)
            Debug.Log($"[{name}] Chegou ao EntrancePoint. Indo para espera.");

        GoToWaitingPoint();
    }

    private void ArriveAtWaitingPoint()
    {
        StopAgent();

        if (reservedSeat != null)
        {
            Transform sitPoint = reservedSeat.SitPoint;

            if (sitPoint != null)
            {
                if (snapToSeatOnArrival)
                    transform.position = sitPoint.position;

                if (rotateToSeatOnArrival)
                    transform.rotation = sitPoint.rotation;
            }
        }

        currentState = ClientState.WaitingForService;
        ShowInteractionIcon();
        SetSit(true);

        AddToQueue();

        if (enableDebugLogs)
            Debug.Log($"[{name}] Agora está WaitingForService. UI pode abrir no clique.");
    }

    private void ArriveAtBarberChairWalkPoint()
    {
        StopAgent();

        if (barberChairSitPoint != null)
        {
            transform.position = barberChairSitPoint.position;
            transform.rotation = barberChairSitPoint.rotation;
        }

        currentState = ClientState.InService;
        SetSit(true);

        if (enableDebugLogs)
            Debug.Log($"[{name}] Sentado na cadeira de barbeiro.");
    }

    private void ArriveAtCashier()
    {
        StopAgent();
        StartCoroutine(CashierRoutine());
    }

    private void ArriveAtExit()
    {
        RemoveFromQueue();
        ReleaseReservedSeat();

        if (BarbershopServiceManager.Instance != null)
            BarbershopServiceManager.Instance.ClearCurrentClient(this);

        NotifySpawnerFinished();
        Destroy(gameObject);
    }

    private IEnumerator CashierRoutine()
    {
        yield return new WaitForSeconds(cashierWaitTime);

        if (BarbershopServiceManager.Instance != null)
            BarbershopServiceManager.Instance.NotifyClientFinishedCashier(this);
        else
            LeaveShop(exitPoint);
    }

    private void AddToQueue()
    {
        if (addedToQueue || BarberQueueSystem.Instance == null)
            return;

        BarberQueueSystem.Instance.AddClientToQueue(this, clientDisplayName, maxPatienceMinutes);
        addedToQueue = true;
    }

    private void RemoveFromQueue()
    {
        if (!addedToQueue)
            return;

        if (BarberQueueSystem.Instance != null)
            BarberQueueSystem.Instance.RemoveClientFromQueue(this);

        addedToQueue = false;
    }

    private void NotifySpawnerFinished()
    {
        if (spawner != null)
            spawner.NotifyClientFinished(this);
    }

    private void SetDestination(Vector3 position)
    {
        if (agent == null || !agent.enabled)
            return;

        Vector3 finalPosition = position;

        if (NavMesh.SamplePosition(position, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
            finalPosition = hit.position;
        else if (enableDebugLogs)
            Debug.LogWarning($"[{name}] Destino fora do NavMesh. Usando posição original: {position}");

        lastDestination = finalPosition;
        ResetStuckDetection();

        agent.isStopped = false;
        agent.SetDestination(finalPosition);
    }

    private void StopAgent()
    {
        if (agent == null || !agent.enabled)
            return;

        agent.isStopped = true;
        agent.ResetPath();
        currentTarget = null;
        ResetStuckDetection();
    }

    private bool HasReachedDestination()
    {
        if (agent == null || !agent.enabled)
            return false;

        if (agent.pathPending)
            return false;

        float safeArrivalDistance = Mathf.Max(arrivalDistance, agent.stoppingDistance + 0.2f);

        if (agent.remainingDistance <= safeArrivalDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude <= 0.05f)
                return true;
        }

        if (currentTarget != null)
        {
            float directDistance = Vector3.Distance(transform.position, currentTarget.position);

            if (directDistance <= forceArrivalDistance)
                return true;
        }

        return false;
    }

    private void UpdateStuckDetection()
    {
        if (agent == null || !agent.enabled || currentTarget == null)
            return;

        stuckCheckTimer += Time.deltaTime;

        if (stuckCheckTimer < stuckCheckInterval)
            return;

        stuckCheckTimer = 0f;

        bool movingTooSlow = agent.velocity.magnitude <= stuckVelocityThreshold;
        bool stillHasDestination = agent.hasPath || agent.pathPending;
        bool closeToTarget = Vector3.Distance(transform.position, currentTarget.position) <= forceArrivalDistance;

        if (movingTooSlow && stillHasDestination)
            stuckTimer += stuckCheckInterval;
        else
            stuckTimer = 0f;

        if (stuckTimer < maxStuckTime)
            return;

        if (autoAdvanceIfStuckNearTarget && closeToTarget)
        {
            if (enableDebugLogs)
                Debug.LogWarning($"[{name}] Travado próximo ao destino no estado {currentState}. Forçando chegada.");

            HandleReachedDestination();
            return;
        }

        if (repathWhenStuck)
        {
            if (enableDebugLogs)
                Debug.LogWarning($"[{name}] Possível travamento no estado {currentState}. Recalculando rota.");

            stuckTimer = 0f;
            SetDestination(lastDestination);
        }
    }

    private void ResetStuckDetection()
    {
        stuckTimer = 0f;
        stuckCheckTimer = 0f;
    }

    private void WarpToNavMeshIfNeeded()
    {
        if (agent == null || !agent.enabled)
            return;

        if (agent.isOnNavMesh)
            return;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);

            if (enableDebugLogs)
                Debug.Log($"[{name}] Reposicionado no NavMesh ao inicializar.");
        }
        else
        {
            Debug.LogWarning($"[{name}] Não está sobre o NavMesh e não encontrou ponto próximo.");
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null || string.IsNullOrWhiteSpace(speedParam))
            return;

        float speed = 0f;

        if (agent != null && agent.enabled)
            speed = agent.velocity.magnitude;

        animator.SetFloat(speedParam, speed);
    }

    private void SetSit(bool value)
    {
        if (animator == null || string.IsNullOrWhiteSpace(sitParam))
            return;

        animator.SetBool(sitParam, value);
    }

    private void ShowInteractionIcon()
    {
        if (interactionIcon != null)
            interactionIcon.SetActive(true);
    }

    private void HideInteractionIcon()
    {
        if (interactionIcon != null)
            interactionIcon.SetActive(false);
    }
}