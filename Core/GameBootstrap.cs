using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    public static GameBootstrap Instance { get; private set; }

    [Header("Tela de Loading Inicial")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private CanvasGroup loadingCanvasGroup;
    [SerializeField] private float loadingFadeOutDuration = 0.4f;

    [Header("Sistemas principais")]
    [SerializeField] private GlobalDialogueManager globalDialogueManager;
    [SerializeField] private GlobalReputationSystem globalReputationSystem;
    [SerializeField] private BarbershopRatingManager barbershopRatingManager;
    [SerializeField] private FinanceManager financeManager;
    [SerializeField] private BarbershopCashRegister barbershopCashRegister;
    [SerializeField] private BarberQueueSystem barberQueueSystem;
    [SerializeField] private ClientAppointmentScheduler appointmentScheduler;

    [Header("UI que será liberada depois")]
    [SerializeField] private GameObject[] uiObjectsToEnableAfterBootstrap;

    [Header("Objetos pesados para ativar depois")]
    [SerializeField] private GameObject[] delayedObjectsToEnable;

    [Header("Performance Mobile")]
    [SerializeField] private bool limitFrameRateOnMobile = true;
    [SerializeField] private int targetMobileFrameRate = 30;
    [SerializeField] private bool disableVSyncOnMobile = true;

    [Header("Carregamento progressivo")]
    [SerializeField] private bool disableUIOnAwake = true;
    [SerializeField] private bool disableDelayedObjectsOnAwake = true;
    [SerializeField] private bool cleanMemoryOnStart = true;
    [SerializeField] private float startDelay = 0.1f;
    [SerializeField] private float uiDelay = 0.25f;
    [SerializeField] private float heavyObjectsDelay = 0.5f;
    [SerializeField] private int heavyObjectsPerBatch = 1;
    [SerializeField] private float delayBetweenHeavyObjectBatches = 0.05f;
    [SerializeField] private bool initializeDelayedObjects = true;

    [Header("Debug")]
    [SerializeField] private bool enableLogs = true;
    [SerializeField] private bool logActivatedObjects = false;
    [SerializeField] private bool logWarnings = true;

    public bool IsReady { get; private set; }
    public bool IsBootstrapping { get; private set; }
    public float Progress01 { get; private set; }

    private readonly List<GameObject> coreObjectsCache = new List<GameObject>(8);
    private Coroutine bootstrapRoutine;

    private void Awake()
    {
        SetupSingleton();

        ShowLoadingScreenImmediate();

        ConfigureMobilePerformance();
        BuildCoreObjectsCache();

        if (disableUIOnAwake)
            SetObjectsActive(uiObjectsToEnableAfterBootstrap, false);

        if (disableDelayedObjectsOnAwake)
            SetObjectsActive(delayedObjectsToEnable, false);

        FindMissingReferences();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        if (bootstrapRoutine != null)
            StopCoroutine(bootstrapRoutine);

        bootstrapRoutine = StartCoroutine(BootstrapRoutine());
    }

    private void SetupSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private IEnumerator BootstrapRoutine()
    {
        IsReady = false;
        IsBootstrapping = true;
        Progress01 = 0f;

        Log("Bootstrap iniciado.");

        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        if (cleanMemoryOnStart)
        {
            Log("Limpando memória antes da inicialização...");
            yield return Resources.UnloadUnusedAssets();
            System.GC.Collect();
            yield return null;
        }

        Progress01 = 0.15f;

        yield return InitializeCoreSystemsRoutine();

        Progress01 = 0.45f;

        if (uiDelay > 0f)
            yield return new WaitForSeconds(uiDelay);

        yield return EnableUIRoutine();

        Progress01 = 0.7f;

        if (heavyObjectsDelay > 0f)
            yield return new WaitForSeconds(heavyObjectsDelay);

        yield return EnableDelayedObjectsRoutine();

        Progress01 = 1f;
        IsReady = true;
        IsBootstrapping = false;
        bootstrapRoutine = null;

        Log("Bootstrap finalizado. Cena pronta.");

        yield return HideLoadingScreenRoutine();
    }

    private void ShowLoadingScreenImmediate()
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.alpha = 1f;
            loadingCanvasGroup.blocksRaycasts = true;
            loadingCanvasGroup.interactable = true;
        }
    }

    private IEnumerator HideLoadingScreenRoutine()
    {
        if (loadingScreen == null)
            yield break;

        if (loadingCanvasGroup == null || loadingFadeOutDuration <= 0f)
        {
            loadingScreen.SetActive(false);
            yield break;
        }

        float elapsed = 0f;
        loadingCanvasGroup.blocksRaycasts = true;
        loadingCanvasGroup.interactable = false;

        while (elapsed < loadingFadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            loadingCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / loadingFadeOutDuration);
            yield return null;
        }

        loadingCanvasGroup.alpha = 0f;
        loadingCanvasGroup.blocksRaycasts = false;
        loadingScreen.SetActive(false);
    }

    private void ConfigureMobilePerformance()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (limitFrameRateOnMobile)
        {
            Application.targetFrameRate = Mathf.Clamp(targetMobileFrameRate, 24, 60);

            if (disableVSyncOnMobile)
                QualitySettings.vSyncCount = 0;
        }
#endif
    }

    private void BuildCoreObjectsCache()
    {
        coreObjectsCache.Clear();

        AddCoreObject(globalDialogueManager);
        AddCoreObject(globalReputationSystem);
        AddCoreObject(barbershopRatingManager);
        AddCoreObject(financeManager);
        AddCoreObject(barbershopCashRegister);
        AddCoreObject(barberQueueSystem);
        AddCoreObject(appointmentScheduler);
    }

    private void AddCoreObject(Component component)
    {
        if (component == null)
            return;

        GameObject obj = component.gameObject;

        if (obj != null && !coreObjectsCache.Contains(obj))
            coreObjectsCache.Add(obj);
    }

    private IEnumerator InitializeCoreSystemsRoutine()
    {
        Log("Inicializando sistemas principais...");

        for (int i = 0; i < coreObjectsCache.Count; i++)
        {
            ActivateAndInitialize(coreObjectsCache[i]);

            if (coreObjectsCache.Count > 0)
                Progress01 = Mathf.Lerp(0.15f, 0.45f, (i + 1f) / coreObjectsCache.Count);

            yield return null;
        }
    }

    private IEnumerator EnableUIRoutine()
    {
        Log("Liberando UI...");

        if (uiObjectsToEnableAfterBootstrap == null)
            yield break;

        for (int i = 0; i < uiObjectsToEnableAfterBootstrap.Length; i++)
        {
            GameObject obj = uiObjectsToEnableAfterBootstrap[i];

            if (obj == null)
                continue;

            obj.SetActive(true);
            InitializeBootstrapComponents(obj);

            if (logActivatedObjects)
                Log("UI ativada: " + obj.name);

            yield return null;
        }
    }

    private IEnumerator EnableDelayedObjectsRoutine()
    {
        Log("Liberando objetos pesados progressivamente...");

        if (delayedObjectsToEnable == null || delayedObjectsToEnable.Length == 0)
            yield break;

        int batchSize = Mathf.Max(1, heavyObjectsPerBatch);
        int activatedInBatch = 0;

        for (int i = 0; i < delayedObjectsToEnable.Length; i++)
        {
            GameObject obj = delayedObjectsToEnable[i];

            if (obj == null)
                continue;

            obj.SetActive(true);

            if (initializeDelayedObjects)
                InitializeBootstrapComponents(obj);

            if (logActivatedObjects)
                Log("Objeto pesado ativado: " + obj.name);

            activatedInBatch++;

            float progress = (i + 1f) / delayedObjectsToEnable.Length;
            Progress01 = Mathf.Lerp(0.7f, 1f, progress);

            if (activatedInBatch >= batchSize)
            {
                activatedInBatch = 0;

                if (delayBetweenHeavyObjectBatches > 0f)
                    yield return new WaitForSeconds(delayBetweenHeavyObjectBatches);
                else
                    yield return null;
            }
        }
    }

    private void ActivateAndInitialize(GameObject obj)
    {
        if (obj == null)
            return;

        if (!obj.activeSelf)
            obj.SetActive(true);

        InitializeBootstrapComponents(obj);

        if (logActivatedObjects)
            Log("Sistema inicializado: " + obj.name);
    }

    private void InitializeBootstrapComponents(GameObject obj)
    {
        if (obj == null)
            return;

        IGameBootstrapInitializable[] initializables =
            obj.GetComponentsInChildren<IGameBootstrapInitializable>(true);

        for (int i = 0; i < initializables.Length; i++)
        {
            if (initializables[i] != null)
                initializables[i].InitializeFromBootstrap();
        }
    }

    private void SetObjectsActive(GameObject[] objects, bool active)
    {
        if (objects == null)
            return;

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
                objects[i].SetActive(active);
        }
    }

    private void FindMissingReferences()
    {
        if (!logWarnings)
            return;

        if (globalDialogueManager == null)
            LogWarning("GlobalDialogueManager não foi atribuído.");

        if (globalReputationSystem == null)
            LogWarning("GlobalReputationSystem não foi atribuído.");

        if (barbershopRatingManager == null)
            LogWarning("BarbershopRatingManager não foi atribuído.");

        if (financeManager == null)
            LogWarning("FinanceManager não foi atribuído.");

        if (barbershopCashRegister == null)
            LogWarning("BarbershopCashRegister não foi atribuído.");

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
        if (enableLogs && logWarnings)
            Debug.LogWarning("[GameBootstrap] " + message);
    }
}