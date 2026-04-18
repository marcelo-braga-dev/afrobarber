using System.Collections.Generic;
using UnityEngine;

public class ServiceDatabase : MonoBehaviour
{
    public enum ServiceLoadMode
    {
        ScriptDefaults,
        Manual
    }

    [Header("Modo")]
    [SerializeField] private ServiceLoadMode loadMode = ServiceLoadMode.ScriptDefaults;

    [Header("Serviços do script")]
    [SerializeField] private List<ClientRequestData> scriptLoadedServices = new List<ClientRequestData>();

    [Header("Serviços manuais")]
    [SerializeField] private List<ClientRequestData> manualServices = new List<ClientRequestData>();

    [Header("Configuração")]
    [SerializeField] private bool autoLoadOnAwake = true;

    public IReadOnlyList<ClientRequestData> ActiveServices =>
        loadMode == ServiceLoadMode.Manual ? manualServices : scriptLoadedServices;

    private void Awake()
    {
        if (autoLoadOnAwake)
        {
            EnsureLoaded();
        }
    }

    private void Reset()
    {
        LoadDefaultsFromScript();
    }

    [ContextMenu("Carregar serviços padrão do script")]
    public void LoadDefaultsFromScript()
    {
        scriptLoadedServices = DefaultServiceLibrary.CreateDefaultServices();
        Debug.Log($"[ServiceDatabase] Serviços carregados do script: {scriptLoadedServices.Count}");
    }

    public void EnsureLoaded()
    {
        if (loadMode == ServiceLoadMode.ScriptDefaults)
        {
            if (scriptLoadedServices == null || scriptLoadedServices.Count == 0)
            {
                LoadDefaultsFromScript();
            }
        }
        else
        {
            if (manualServices == null)
                manualServices = new List<ClientRequestData>();
        }
    }

    public ClientRequestData GetRandomService()
    {
        EnsureLoaded();

        List<ClientRequestData> source = loadMode == ServiceLoadMode.Manual
            ? manualServices
            : scriptLoadedServices;

        List<ClientRequestData> valid = new List<ClientRequestData>();

        for (int i = 0; i < source.Count; i++)
        {
            if (source[i] != null)
                valid.Add(source[i]);
        }

        if (valid.Count == 0)
        {
            Debug.LogWarning($"[ServiceDatabase] Nenhum serviço válido no modo {loadMode}.");
            return null;
        }

        int index = Random.Range(0, valid.Count);
        return valid[index];
    }

    public ServiceLoadMode GetLoadMode()
    {
        return loadMode;
    }

    public int GetServiceCount()
    {
        EnsureLoaded();

        List<ClientRequestData> source = loadMode == ServiceLoadMode.Manual
            ? manualServices
            : scriptLoadedServices;

        int count = 0;

        for (int i = 0; i < source.Count; i++)
        {
            if (source[i] != null)
                count++;
        }

        return count;
    }
}