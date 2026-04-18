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
    [SerializeField] private float arrivalDistance = 0.35f;
    [SerializeField] private float cashierWaitTime = 2f;
    [SerializeField] private float maxPatienceMinutes = 90f;

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

    public string ClientDisplayName => clientDisplayName;
    public ClientRequestData CurrentRequest => currentRequest;
    public ClientRequestData RequestData => currentRequest;
    public ClientState CurrentState => currentState;
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
            HandleReachedDestination();
    }
    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }

    public bool HasFixedProfileRequest()
    {
        return serviceProfile != null && serviceProfile.requestData != null;
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
            currentRequest = serviceProfile.requestData;
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

        if (serviceProfile != null)
            hairVisualController.ApplyBeforeHair(serviceProfile);
    }

    public void ApplyFinalHair()
    {
        if (finalHairApplied)
            return;

        if (hairVisualController == null)
            return;

        if (serviceProfile == null)
            return;

        hairVisualController.ApplyAfterHair(serviceProfile);
        finalHairApplied = true;
    }

    public AfroCutInfo GetCurrentCutInfo()
    {
        if (currentRequest == null)
            return null;

        if (EducationProgressManager.Instance == null)
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
        SetDestination(barberChairWalkPoint.position);
        currentState = ClientState.GoingToBarberChair;

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
        SetDestination(cashierPoint.position);
        currentState = ClientState.GoingToCashier;
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
        SetDestination(targetExit.position);
        currentState = ClientState.Leaving;
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
        SetDestination(entrancePoint.position);
        currentState = ClientState.GoingToEntrance;
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
        SetDestination(waitingApproachPoint.position);
        currentState = ClientState.GoingToWaitingPoint;

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
        if (addedToQueue)
            return;

        if (BarberQueueSystem.Instance == null)
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

        agent.isStopped = false;
        agent.SetDestination(position);
    }

    private void StopAgent()
    {
        if (agent == null || !agent.enabled)
            return;

        agent.isStopped = true;
        agent.ResetPath();
        currentTarget = null;
    }

    private bool HasReachedDestination()
    {
        if (agent == null || !agent.enabled)
            return false;

        if (agent.pathPending)
            return false;

        if (agent.remainingDistance > arrivalDistance)
            return false;

        if (agent.hasPath && agent.velocity.sqrMagnitude > 0.01f)
            return false;

        return true;
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