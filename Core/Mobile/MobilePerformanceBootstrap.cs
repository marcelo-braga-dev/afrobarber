using UnityEngine;
using UnityEngine.Rendering;

public class MobilePerformanceBootstrap : MonoBehaviour
{
    [Header("Aplicação")]
    [SerializeField] private int targetFrameRate = 30;
    [SerializeField] private bool disableVSync = true;

    [Header("Qualidade")]
    [SerializeField] private string androidQualityName = "Mobile Medium";

    [Header("Memória")]
    [SerializeField] private bool triggerGcOnSceneStart = true;

    [Header("Debug")]
    [SerializeField] private bool verboseLogs;

    private void Awake()
    {
        ApplyRuntimeSettings();
    }

    private void ApplyRuntimeSettings()
    {
        if (disableVSync)
            QualitySettings.vSyncCount = 0;

        Application.targetFrameRate = Mathf.Max(20, targetFrameRate);

#if UNITY_ANDROID
        TrySetQuality(androidQualityName);
#elif UNITY_IOS
        TrySetQuality(iosQualityName);
#endif

#if UNITY_ANDROID || UNITY_IOS
        QualitySettings.maxQueuedFrames = 2;
#endif

        if (triggerGcOnSceneStart)
        {
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        if (verboseLogs)
        {
            Debug.Log($"[MobilePerformanceBootstrap] targetFrameRate={Application.targetFrameRate} quality={QualitySettings.names[QualitySettings.GetQualityLevel()]}");
            Debug.Log($"[MobilePerformanceBootstrap] vSync={QualitySettings.vSyncCount} maxQueuedFrames={QualitySettings.maxQueuedFrames} colorSpace={QualitySettings.activeColorSpace}");
        }
    }

    private void TrySetQuality(string qualityName)
    {
        if (string.IsNullOrWhiteSpace(qualityName))
            return;

        string[] qualityNames = QualitySettings.names;
        for (int i = 0; i < qualityNames.Length; i++)
        {
            if (!qualityNames[i].Equals(qualityName))
                continue;

            QualitySettings.SetQualityLevel(i, true);
            return;
        }

        if (verboseLogs)
            Debug.LogWarning($"[MobilePerformanceBootstrap] Perfil de qualidade '{qualityName}' não encontrado.");
    }
}
