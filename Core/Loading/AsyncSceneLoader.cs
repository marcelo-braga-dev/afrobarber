using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AsyncSceneLoader : MonoBehaviour
{
    [Serializable]
    public class SceneLoadProfile
    {
        public string sceneName = "GameScene";
        [Range(0f, 1f)] public float activationProgress = 0.9f;
        public float minimumLoadingScreenSeconds = 0.75f;
    }

    [Header("Configuração")]
    [SerializeField] private SceneLoadProfile profile = new SceneLoadProfile();
    [SerializeField] private bool unloadUnusedAssetsBeforeLoad = true;

    [Header("UI de Loading")]
    [SerializeField] private GameObject loadingRoot;

    [Header("Debug")]
    [SerializeField] private bool logInEditorOnly = true;

    public bool IsLoading { get; private set; }
    public float Progress01 { get; private set; }

    public event Action<float> OnProgressChanged;
    public event Action<string> OnSceneLoaded;

    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("[AsyncSceneLoader] Nome da cena vazio.");
            return;
        }

        profile.sceneName = sceneName;
        LoadConfiguredScene();
    }

    public void LoadConfiguredScene()
    {
        if (IsLoading)
            return;

        StartCoroutine(LoadRoutine());
    }

    private IEnumerator LoadRoutine()
    {
        IsLoading = true;
        Progress01 = 0f;
        SetLoadingRoot(true);

        float startTime = Time.realtimeSinceStartup;

        if (unloadUnusedAssetsBeforeLoad)
        {
            yield return Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(profile.sceneName, LoadSceneMode.Single);
        if (loadOperation == null)
        {
            Debug.LogError($"[AsyncSceneLoader] Não foi possível iniciar o carregamento da cena '{profile.sceneName}'.");
            SetLoadingRoot(false);
            IsLoading = false;
            yield break;
        }

        loadOperation.allowSceneActivation = false;

        while (!loadOperation.isDone)
        {
            float progress = Mathf.Clamp01(loadOperation.progress / Mathf.Max(0.01f, profile.activationProgress));
            SetProgress(progress);

            bool reachedActivationProgress = loadOperation.progress >= profile.activationProgress;
            bool elapsedMinimumLoadingTime = Time.realtimeSinceStartup - startTime >= profile.minimumLoadingScreenSeconds;

            if (reachedActivationProgress && elapsedMinimumLoadingTime)
            {
                loadOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        SetProgress(1f);
        Log($"Cena '{profile.sceneName}' carregada com sucesso.");
        OnSceneLoaded?.Invoke(profile.sceneName);
        IsLoading = false;
    }

    private void SetProgress(float value)
    {
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

    private void Log(string message)
    {
#if UNITY_EDITOR
        Debug.Log($"[AsyncSceneLoader] {message}");
#else
        if (!logInEditorOnly)
            Debug.Log($"[AsyncSceneLoader] {message}");
#endif
    }
}
