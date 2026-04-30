using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AsyncSceneLoader : MonoBehaviour
{
    [Serializable]
    public class SceneLoadProfile
    {
        public string sceneName = "GameScene";

        [Tooltip("Normalmente o LoadSceneAsync para em 0.9 antes da ativação.")]
        [Range(0f, 1f)] public float activationProgress = 0.9f;

        [Tooltip("Tempo mínimo da tela de carregamento para evitar transição seca.")]
        public float minimumLoadingScreenSeconds = 0.75f;
    }

    [Header("Configuração")]
    [SerializeField] private SceneLoadProfile profile = new SceneLoadProfile();

    [SerializeField] private bool unloadUnusedAssetsBeforeLoad = true;
    [SerializeField] private bool collectGarbageBeforeLoad = true;

    [Header("Preload")]
    [SerializeField] private List<string> scenesToPreloadOnAwake = new List<string>();
    [SerializeField] private bool preloadOnAwake = true;
    [SerializeField] private bool keepOnlyOnePreloadedScene = true;

    [Header("UI de Loading")]
    [SerializeField] private GameObject loadingRoot;

    [Header("Debug")]
    [SerializeField] private bool enableLogs = true;
    [SerializeField] private bool logInEditorOnly = true;

    public bool IsLoading { get; private set; }
    public bool IsPreloading { get; private set; }
    public float Progress01 { get; private set; }

    public event Action<float> OnProgressChanged;
    public event Action<string> OnSceneLoaded;

    private Coroutine preloadQueueCoroutine;
    private Coroutine loadCoroutine;

    private readonly Dictionary<string, AsyncOperation> preloadedOperations =
        new Dictionary<string, AsyncOperation>(StringComparer.Ordinal);

    private readonly Queue<string> preloadQueue = new Queue<string>();

    private readonly HashSet<string> scheduledPreloads =
        new HashSet<string>(StringComparer.Ordinal);

    private void Awake()
    {
        SetLoadingRoot(false);

        if (!preloadOnAwake || scenesToPreloadOnAwake == null || scenesToPreloadOnAwake.Count == 0)
            return;

        for (int i = 0; i < scenesToPreloadOnAwake.Count; i++)
        {
            string sceneName = scenesToPreloadOnAwake[i];

            if (string.IsNullOrWhiteSpace(sceneName))
                continue;

            EnqueuePreload(sceneName);
        }

        StartPreloadQueueIfNeeded();
    }

    private void OnDestroy()
    {
        ClearCallbacks();
    }

    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            LogWarning("Nome da cena vazio.");
            return;
        }

        profile.sceneName = sceneName;
        LoadConfiguredScene();
    }

    public void LoadConfiguredScene()
    {
        if (IsLoading)
        {
            LogWarning("Já existe um carregamento em andamento.");
            return;
        }

        if (string.IsNullOrWhiteSpace(profile.sceneName))
        {
            LogWarning("Nenhuma cena configurada para carregar.");
            return;
        }

        if (TryActivatePreloadedScene(profile.sceneName))
            return;

        loadCoroutine = StartCoroutine(LoadRoutine(profile.sceneName));
    }

    public void PreloadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        if (IsLoading)
            return;

        if (preloadedOperations.ContainsKey(sceneName))
            return;

        EnqueuePreload(sceneName);
        StartPreloadQueueIfNeeded();
    }

    public void ClearPreloadedScenes()
    {
        if (preloadedOperations.Count == 0)
            return;

        string[] keys = new string[preloadedOperations.Count];
        preloadedOperations.Keys.CopyTo(keys, 0);

        for (int i = 0; i < keys.Length; i++)
            UnloadPreloadedScene(keys[i]);

        scheduledPreloads.Clear();
        preloadQueue.Clear();
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        IsLoading = true;
        Progress01 = 0f;
        SetProgress(0f);
        SetLoadingRoot(true);

        float startTime = Time.realtimeSinceStartup;

        yield return ReleaseMemoryIfNeeded();

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        if (loadOperation == null)
        {
            LogError($"Não foi possível iniciar o carregamento da cena '{sceneName}'.");
            SetLoadingRoot(false);
            IsLoading = false;
            loadCoroutine = null;
            yield break;
        }

        loadOperation.allowSceneActivation = false;

        while (!loadOperation.isDone)
        {
            float progress = Mathf.Clamp01(loadOperation.progress / Mathf.Max(0.01f, profile.activationProgress));
            SetProgress(progress);

            bool reachedActivationProgress = loadOperation.progress >= profile.activationProgress;
            bool elapsedMinimumLoadingTime =
                Time.realtimeSinceStartup - startTime >= profile.minimumLoadingScreenSeconds;

            if (reachedActivationProgress && elapsedMinimumLoadingTime)
                loadOperation.allowSceneActivation = true;

            yield return null;
        }

        SetProgress(1f);
        Log($"Cena '{sceneName}' carregada com sucesso.");

        OnSceneLoaded?.Invoke(sceneName);

        IsLoading = false;
        loadCoroutine = null;
        SetLoadingRoot(false);
    }

    private IEnumerator PreloadQueueRoutine()
    {
        if (IsLoading)
        {
            preloadQueueCoroutine = null;
            yield break;
        }

        IsPreloading = true;

        while (preloadQueue.Count > 0)
        {
            if (IsLoading)
                break;

            string sceneName = preloadQueue.Dequeue();
            scheduledPreloads.Remove(sceneName);

            if (string.IsNullOrWhiteSpace(sceneName))
                continue;

            if (preloadedOperations.ContainsKey(sceneName))
                continue;

            if (keepOnlyOnePreloadedScene)
                UnloadAnyOtherPreloadedScenes(sceneName);

            yield return StartCoroutine(PreloadSceneRoutine(sceneName));
        }

        IsPreloading = false;
        preloadQueueCoroutine = null;
    }

    private IEnumerator PreloadSceneRoutine(string sceneName)
    {
        yield return ReleaseMemoryIfNeeded();

        AsyncOperation preloadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        if (preloadOperation == null)
        {
            LogWarning($"Falha ao pré-carregar cena '{sceneName}'.");
            yield break;
        }

        preloadOperation.allowSceneActivation = false;

        while (preloadOperation.progress < profile.activationProgress)
            yield return null;

        preloadedOperations[sceneName] = preloadOperation;

        Log($"Cena '{sceneName}' pré-carregada até {profile.activationProgress * 100f:0}%.");
    }

    private IEnumerator ActivatePreloadedSceneRoutine(string sceneName, AsyncOperation preloadedOperation)
    {
        IsLoading = true;
        Progress01 = 0f;
        SetProgress(0f);
        SetLoadingRoot(true);

        float startTime = Time.realtimeSinceStartup;

        while (Time.realtimeSinceStartup - startTime < profile.minimumLoadingScreenSeconds)
            yield return null;

        preloadedOperation.allowSceneActivation = true;

        while (!preloadedOperation.isDone)
        {
            float progress = Mathf.Clamp01(preloadedOperation.progress / Mathf.Max(0.01f, profile.activationProgress));
            SetProgress(progress);
            yield return null;
        }

        SetProgress(1f);

        preloadedOperations.Remove(sceneName);
        OnSceneLoaded?.Invoke(sceneName);

        IsLoading = false;
        loadCoroutine = null;
        SetLoadingRoot(false);

        if (keepOnlyOnePreloadedScene)
            UnloadAnyOtherPreloadedScenes(sceneName);

        Log($"Cena pré-carregada '{sceneName}' ativada com sucesso.");
    }

    private IEnumerator ReleaseMemoryIfNeeded()
    {
        if (unloadUnusedAssetsBeforeLoad)
            yield return Resources.UnloadUnusedAssets();

        if (collectGarbageBeforeLoad)
            GC.Collect();

        yield return null;
    }

    private void EnqueuePreload(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        if (preloadedOperations.ContainsKey(sceneName))
            return;

        if (!scheduledPreloads.Add(sceneName))
            return;

        preloadQueue.Enqueue(sceneName);
    }

    private void StartPreloadQueueIfNeeded()
    {
        if (preloadQueueCoroutine != null)
            return;

        if (preloadQueue.Count == 0)
            return;

        if (IsLoading)
            return;

        preloadQueueCoroutine = StartCoroutine(PreloadQueueRoutine());
    }

    private bool TryActivatePreloadedScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return false;

        if (!preloadedOperations.TryGetValue(sceneName, out AsyncOperation preloadedOperation))
            return false;

        loadCoroutine = StartCoroutine(ActivatePreloadedSceneRoutine(sceneName, preloadedOperation));
        return true;
    }

    private void UnloadAnyOtherPreloadedScenes(string keepSceneName)
    {
        if (preloadedOperations.Count == 0)
            return;

        string[] keys = new string[preloadedOperations.Count];
        preloadedOperations.Keys.CopyTo(keys, 0);

        for (int i = 0; i < keys.Length; i++)
        {
            string sceneName = keys[i];

            if (sceneName == keepSceneName)
                continue;

            UnloadPreloadedScene(sceneName);
        }
    }

    private void UnloadPreloadedScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        Scene loadedScene = SceneManager.GetSceneByName(sceneName);

        if (loadedScene.IsValid() && loadedScene.isLoaded)
            SceneManager.UnloadSceneAsync(loadedScene);

        preloadedOperations.Remove(sceneName);
    }

    private void SetProgress(float value)
    {
        value = Mathf.Clamp01(value);

        if (Mathf.Approximately(Progress01, value))
            return;

        Progress01 = value;
        OnProgressChanged?.Invoke(Progress01);
    }

    private void SetLoadingRoot(bool state)
    {
        if (loadingRoot != null)
            loadingRoot.SetActive(state);
    }

    private void ClearCallbacks()
    {
        OnProgressChanged = null;
        OnSceneLoaded = null;
    }

    private void Log(string message)
    {
        if (!enableLogs)
            return;

#if UNITY_EDITOR
        Debug.Log($"[AsyncSceneLoader] {message}");
#else
        if (!logInEditorOnly)
            Debug.Log($"[AsyncSceneLoader] {message}");
#endif
    }

    private void LogWarning(string message)
    {
        if (!enableLogs)
            return;

#if UNITY_EDITOR
        Debug.LogWarning($"[AsyncSceneLoader] {message}");
#else
        if (!logInEditorOnly)
            Debug.LogWarning($"[AsyncSceneLoader] {message}");
#endif
    }

    private void LogError(string message)
    {
#if UNITY_EDITOR
        Debug.LogError($"[AsyncSceneLoader] {message}");
#else
        Debug.LogError($"[AsyncSceneLoader] {message}");
#endif
    }
}