using System.Collections.Generic;
using UnityEngine;

public class TimeResponsiveMaterial : MonoBehaviour
{
    [System.Serializable]
    public class VisualSettings
    {
        [Header("Nome do período")]
        public string periodName = "Manhã";

        [Header("Cor principal")]
        public Color baseColor = Color.white;

        [Header("Multiplicador da cor")]
        [Range(0f, 3f)] public float colorMultiplier = 1f;

        [Header("Emissão")]
        public bool changeEmission = false;
        public Color emissionColor = Color.black;
        [Range(0f, 5f)] public float emissionIntensity = 0f;
    }

    [Header("Renderers que serão alterados")]
    [SerializeField] private Renderer[] targetRenderers;

    [Header("Nome da propriedade de cor do shader")]
    [SerializeField] private string colorPropertyName = "_BaseColor";

    [Header("Nome da propriedade de emissão do shader")]
    [SerializeField] private string emissionPropertyName = "_EmissionColor";

    [Header("Visuais por período")]
    [SerializeField] private List<VisualSettings> visuals = new List<VisualSettings>();

    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();

        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            targetRenderers = GetComponentsInChildren<Renderer>(true);
        }
    }

    public void ApplyVisual(string periodName)
    {
        VisualSettings settings = GetSettings(periodName);

        if (settings == null || targetRenderers == null)
            return;

        foreach (Renderer rend in targetRenderers)
        {
            if (rend == null)
                continue;

            rend.GetPropertyBlock(propertyBlock);

            Color finalColor = settings.baseColor * settings.colorMultiplier;
            propertyBlock.SetColor(colorPropertyName, finalColor);

            if (settings.changeEmission)
            {
                Color finalEmission = settings.emissionColor * settings.emissionIntensity;
                propertyBlock.SetColor(emissionPropertyName, finalEmission);

                foreach (Material mat in rend.materials)
                {
                    if (mat != null)
                        mat.EnableKeyword("_EMISSION");
                }
            }
            else
            {
                propertyBlock.SetColor(emissionPropertyName, Color.black);

                foreach (Material mat in rend.materials)
                {
                    if (mat != null)
                        mat.DisableKeyword("_EMISSION");
                }
            }

            rend.SetPropertyBlock(propertyBlock);
        }
    }

    private VisualSettings GetSettings(string periodName)
    {
        if (visuals == null || visuals.Count == 0)
            return null;

        foreach (VisualSettings item in visuals)
        {
            if (item == null)
                continue;

            if (string.Equals(item.periodName, periodName, System.StringComparison.OrdinalIgnoreCase))
                return item;
        }

        return null;
    }

    [ContextMenu("Teste/Aplicar Primeiro Visual")]
    private void TestApplyFirst()
    {
        if (visuals != null && visuals.Count > 0 && visuals[0] != null)
        {
            ApplyVisual(visuals[0].periodName);
        }
    }
}