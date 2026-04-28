using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServiceSelectionMemory : MonoBehaviour
{
    public static ServiceSelectionMemory Instance { get; private set; }

    private const string Prefix = "AFROBARBER_SERVICE_SELECTION_";
    private const string IndexKey = "AFROBARBER_SERVICE_SELECTION_INDEX";
    private static readonly char[] IndexSeparator = { '|' };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            PersistentGameObject.MakePersistent(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private string BuildKey(string serviceId, string requirementId)
    {
        return $"{Prefix}{serviceId}_{requirementId}";
    }

    public void SaveLastProductId(string serviceId, string requirementId, string productId)
    {
        if (string.IsNullOrWhiteSpace(serviceId) ||
            string.IsNullOrWhiteSpace(requirementId) ||
            string.IsNullOrWhiteSpace(productId))
        {
            return;
        }

        string key = BuildKey(serviceId, requirementId);

        PlayerPrefs.SetString(key, productId);
        RegisterKey(key);
        PlayerPrefs.Save();
    }

    public string GetLastProductId(string serviceId, string requirementId)
    {
        if (string.IsNullOrWhiteSpace(serviceId) || string.IsNullOrWhiteSpace(requirementId))
            return string.Empty;

        return PlayerPrefs.GetString(BuildKey(serviceId, requirementId), string.Empty);
    }

    public bool HasSavedSelection(string serviceId, string requirementId)
    {
        if (string.IsNullOrWhiteSpace(serviceId) || string.IsNullOrWhiteSpace(requirementId))
            return false;

        return PlayerPrefs.HasKey(BuildKey(serviceId, requirementId));
    }

    public void RemoveLastProductId(string serviceId, string requirementId)
    {
        if (string.IsNullOrWhiteSpace(serviceId) || string.IsNullOrWhiteSpace(requirementId))
            return;

        string key = BuildKey(serviceId, requirementId);

        PlayerPrefs.DeleteKey(key);
        UnregisterKey(key);
        PlayerPrefs.Save();
    }

    public void ClearAllSelections()
    {
        List<string> keys = GetRegisteredKeys();

        foreach (string key in keys)
        {
            PlayerPrefs.DeleteKey(key);
        }

        PlayerPrefs.DeleteKey(IndexKey);
        PlayerPrefs.Save();

        Debug.Log("[ServiceSelectionMemory] Todas as seleções salvas foram apagadas.");
    }

    private void RegisterKey(string key)
    {
        List<string> keys = GetRegisteredKeys();

        if (!keys.Contains(key))
        {
            keys.Add(key);
            SaveKeyIndex(keys);
        }
    }

    private void UnregisterKey(string key)
    {
        List<string> keys = GetRegisteredKeys();

        if (keys.Remove(key))
        {
            SaveKeyIndex(keys);
        }
    }

    private List<string> GetRegisteredKeys()
    {
        string raw = PlayerPrefs.GetString(IndexKey, string.Empty);

        if (string.IsNullOrWhiteSpace(raw))
            return new List<string>();

        return raw
            .Split(IndexSeparator)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();
    }

    private void SaveKeyIndex(List<string> keys)
    {
        if (keys == null || keys.Count == 0)
        {
            PlayerPrefs.DeleteKey(IndexKey);
            return;
        }

        string raw = string.Join("|", keys.Distinct());
        PlayerPrefs.SetString(IndexKey, raw);
    }
}