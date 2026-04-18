using System.Collections.Generic;
using UnityEngine;

public class SkyboxTimeController : MonoBehaviour
{
    [System.Serializable]
    public class SkyboxPeriodSettings
    {
        [Header("Identificação")]
        public string periodName = "Novo Período";

        [Header("Horário")]
        [Range(0, 23)] public int startHour = 8;
        [Range(0, 23)] public int endHour = 11;

        [Header("Skybox")]
        public Material skyboxMaterial;

        [Header("Sol")]
        public Color sunColor = Color.white;
        [Range(0f, 5f)] public float sunIntensity = 1f;
        public Vector3 sunRotation = new Vector3(50f, -30f, 0f);

        [Header("Ambiente")]
        [Range(0f, 8f)] public float ambientIntensity = 1f;
        public Color ambientLightColor = Color.white;

        [Header("Fog opcional")]
        public bool overrideFog = false;
        public bool fogEnabled = false;
        public Color fogColor = Color.gray;
        [Range(0f, 0.1f)] public float fogDensity = 0.01f;
    }

    [Header("Referências")]
    [SerializeField] private Light sunLight;

    [Header("Períodos configuráveis")]
    [SerializeField] private List<SkyboxPeriodSettings> periods = new List<SkyboxPeriodSettings>();

    [Header("Objetos que respondem ao horário")]
    [SerializeField] private TimeResponsiveMaterial[] responsiveMaterials;
    [SerializeField] private bool autoFindResponsiveMaterialsOnStart = true;

    [Header("Opções")]
    [SerializeField] private bool updateContinuously = false;
    [SerializeField] private bool updateOnDisplayedTimeChangedEvent = true;
    [SerializeField] private bool forceApplyOnStart = true;

    private SkyboxPeriodSettings currentPeriod;
    private bool hasAppliedAtLeastOnce = false;

    private void Start()
    {
        if (autoFindResponsiveMaterialsOnStart)
        {
            responsiveMaterials = Object.FindObjectsByType<TimeResponsiveMaterial>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );
        }

        if (forceApplyOnStart)
        {
            UpdateSkyVisual();
        }

        if (updateOnDisplayedTimeChangedEvent && GameTimeSystem.Instance != null)
        {
            GameTimeSystem.Instance.onDisplayedTimeChanged.AddListener(UpdateSkyVisual);
        }
    }

    private void Update()
    {
        if (updateContinuously)
        {
            UpdateSkyVisual();
        }
    }

    private void OnDestroy()
    {
        if (updateOnDisplayedTimeChangedEvent && GameTimeSystem.Instance != null)
        {
            GameTimeSystem.Instance.onDisplayedTimeChanged.RemoveListener(UpdateSkyVisual);
        }
    }

    public void UpdateSkyVisual()
    {
        int currentHour = GetCurrentGameHour();
        SkyboxPeriodSettings newPeriod = GetCurrentPeriod(currentHour);

        if (newPeriod == null)
        {
            Debug.LogWarning("[SkyboxTimeController] Nenhum período configurado corresponde ao horário atual: " + currentHour);
            return;
        }

        if (hasAppliedAtLeastOnce && currentPeriod == newPeriod && !updateContinuously)
            return;

        currentPeriod = newPeriod;
        hasAppliedAtLeastOnce = true;

        ApplySettings(currentPeriod);
        ApplyResponsiveMaterials(currentPeriod.periodName);
    }

    private int GetCurrentGameHour()
    {
        if (GameTimeSystem.Instance != null)
            return GameTimeSystem.Instance.DisplayedHour;

        return System.DateTime.Now.Hour;
    }

    private SkyboxPeriodSettings GetCurrentPeriod(int hour)
    {
        if (periods == null || periods.Count == 0)
            return null;

        foreach (SkyboxPeriodSettings period in periods)
        {
            if (period == null)
                continue;

            if (IsHourInsidePeriod(hour, period.startHour, period.endHour))
                return period;
        }

        return null;
    }

    private bool IsHourInsidePeriod(int hour, int startHour, int endHour)
    {
        if (startHour == endHour)
            return true;

        if (startHour < endHour)
        {
            return hour >= startHour && hour <= endHour;
        }

        return hour >= startHour || hour <= endHour;
    }

    private void ApplySettings(SkyboxPeriodSettings settings)
    {
        if (settings == null)
            return;

        ApplySkybox(settings);
        ApplySun(settings);
        ApplyAmbient(settings);
        ApplyFog(settings);
    }

    private void ApplySkybox(SkyboxPeriodSettings settings)
    {
        if (settings.skyboxMaterial != null)
        {
            RenderSettings.skybox = settings.skyboxMaterial;
            DynamicGI.UpdateEnvironment();
        }
    }

    private void ApplySun(SkyboxPeriodSettings settings)
    {
        if (sunLight == null)
            return;

        sunLight.color = settings.sunColor;
        sunLight.intensity = settings.sunIntensity;
        sunLight.transform.rotation = Quaternion.Euler(settings.sunRotation);
    }

    private void ApplyAmbient(SkyboxPeriodSettings settings)
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = settings.ambientLightColor;
        RenderSettings.ambientIntensity = settings.ambientIntensity;
    }

    private void ApplyFog(SkyboxPeriodSettings settings)
    {
        if (!settings.overrideFog)
            return;

        RenderSettings.fog = settings.fogEnabled;
        RenderSettings.fogColor = settings.fogColor;
        RenderSettings.fogDensity = settings.fogDensity;
    }

    private void ApplyResponsiveMaterials(string periodName)
    {
        if (responsiveMaterials == null)
            return;

        foreach (TimeResponsiveMaterial item in responsiveMaterials)
        {
            if (item == null)
                continue;

            item.ApplyVisual(periodName);
        }
    }

    public void ForceApplyByIndex(int index)
    {
        if (periods == null || index < 0 || index >= periods.Count || periods[index] == null)
        {
            Debug.LogWarning("[SkyboxTimeController] Índice inválido para ForceApplyByIndex: " + index);
            return;
        }

        currentPeriod = periods[index];
        hasAppliedAtLeastOnce = true;

        ApplySettings(currentPeriod);
        ApplyResponsiveMaterials(currentPeriod.periodName);
    }

    [ContextMenu("Teste/Atualizar Pelo Horário Atual")]
    private void TestUpdateByGameTime()
    {
        UpdateSkyVisual();
    }
}