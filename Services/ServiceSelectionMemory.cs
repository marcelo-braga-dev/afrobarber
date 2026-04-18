using UnityEngine;

public class ServiceSelectionMemory : MonoBehaviour
{
    public static ServiceSelectionMemory Instance { get; private set; }

    private const string Prefix = "AFROBARBER_SERVICE_SELECTION_";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
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
            return;

        PlayerPrefs.SetString(BuildKey(serviceId, requirementId), productId);
        PlayerPrefs.Save();
    }

    public string GetLastProductId(string serviceId, string requirementId)
    {
        if (string.IsNullOrWhiteSpace(serviceId) || string.IsNullOrWhiteSpace(requirementId))
            return "";

        return PlayerPrefs.GetString(BuildKey(serviceId, requirementId), "");
    }
}