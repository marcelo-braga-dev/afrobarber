using System.Collections;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    public static GameBootstrap Instance { get; private set; }

    [Header("Sistemas principais")]
    [SerializeField] private GlobalDialogueManager globalDialogueManager;
    [SerializeField] private GlobalReputationSystem globalReputationSystem;
    [SerializeField] private BarbershopRatingManager barbershopRatingManager;
    [SerializeField] private FinanceManager financeManager;
    [SerializeField] private BarberQueueSystem barberQueueSystem;
    [SerializeField] private ClientAppointmentScheduler appointmentScheduler;

    [Header("UI que será liberada depois")]
    [SerializeField] private GameObject[] uiObjectsToEnableAfterBootstrap;

    [Header("Objetos pesados para ativar depois")]
    [SerializeField] private GameObject[] delayedObjectsToEnable;

    [Header("Configuração")]
    [SerializeField] private bool disableUIOnAwake = true;
    [SerializeField] private float uiDelay = 0.3f;
    [SerializeField] private float heavyObjectsDelay = 0.8f;
    [SerializeField] private bool cleanMemoryOnStart = true;
    [SerializeField] private bool limitFrameRateOnMobile = true;
    [SerializeField] private int targetMobileFrameRate = 30;

    [Header("Debug")]
    [SerializeField] private bool enableLogs = true;

    public bool IsReady { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ConfigureMobilePerformance();

        if (disableUIOnAwake)
            SetObjectsActive(uiObjectsToEnableAfterBootstrap, false);

        SetObjectsActive(delayedObjectsToEnable, false);

        FindMissingReferences();
    }

    private IEnumerator Start()
    {
        IsReady = false;

        if (cleanMemoryOnStart)
        {
            yield return Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        yield return null;

        InitializeCoreSystems();

        yield return new WaitForSeconds(uiDelay);

        EnableUI();

        yield return new WaitForSeconds(heavyObjectsDelay);

        EnableDelayedObjects();

        IsReady = true;

        Log("Bootstrap finalizado. Cena pronta.");
    }

    private void ConfigureMobilePerformance()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (limitFrameRateOnMobile)
        {
            Application.targetFrameRate = targetMobileFrameRate;
            QualitySettings.vSyncCount = 0;
        }
#endif
    }

    private void InitializeCoreSystems()
    {
        Log("Inicializando sistemas principais...");

        if (globalDialogueManager != null)
            globalDialogueManager.gameObject.SetActive(true);

        if (globalReputationSystem != null)
            globalReputationSystem.gameObject.SetActive(true);

        if (barbershopRatingManager != null)
            barbershopRatingManager.gameObject.SetActive(true);

        if (financeManager != null)
            financeManager.gameObject.SetActive(true);

        if (barberQueueSystem != null)
            barberQueueSystem.gameObject.SetActive(true);

        if (appointmentScheduler != null)
            appointmentScheduler.gameObject.SetActive(true);
    }

    private void EnableUI()
    {
        Log("Liberando UI...");

        SetObjectsActive(uiObjectsToEnableAfterBootstrap, true);

        foreach (GameObject obj in uiObjectsToEnableAfterBootstrap)
        {
            if (obj == null)
                continue;

            IGameBootstrapInitializable[] initializables =
                obj.GetComponentsInChildren<IGameBootstrapInitializable>(true);

            foreach (IGameBootstrapInitializable item in initializables)
                item.InitializeFromBootstrap();
        }
    }

    private void EnableDelayedObjects()
    {
        Log("Liberando objetos pesados...");

        SetObjectsActive(delayedObjectsToEnable, true);
    }

    private void SetObjectsActive(GameObject[] objects, bool active)
    {
        if (objects == null)
            return;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }

    private void FindMissingReferences()
    {
        if (globalDialogueManager == null)
            LogWarning("GlobalDialogueManager não foi atribuído.");

        if (globalReputationSystem == null)
            LogWarning("GlobalReputationSystem não foi atribuído.");

        if (barbershopRatingManager == null)
            LogWarning("BarbershopRatingManager não foi atribuído.");

        if (financeManager == null)
            LogWarning("FinanceManager não foi atribuído.");

        if (barberQueueSystem == null)
            LogWarning("BarberQueueSystem não foi atribuído.");

        if (appointmentScheduler == null)
            LogWarning("ClientAppointmentScheduler não foi atribuído.");
    }

    private void Log(string message)
    {
        if (enableLogs)
            Debug.Log("[GameBootstrap] " + message);
    }

    private void LogWarning(string message)
    {
        if (enableLogs)
            Debug.LogWarning("[GameBootstrap] " + message);
    }
}