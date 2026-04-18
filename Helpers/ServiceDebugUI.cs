using UnityEngine;
using UnityEngine.UI;

public class ServiceDebugUI : MonoBehaviour
{
    [SerializeField] private Button completeServiceButton;

    private void Awake()
    {
        if (completeServiceButton != null)
            completeServiceButton.onClick.AddListener(CompleteService);
    }

    public void CompleteService()
    {
        if (BarbershopServiceManager.Instance == null)
        {
            Debug.LogWarning("BarbershopServiceManager.Instance não encontrado.");
            return;
        }

        BarbershopServiceManager.Instance.CompleteCurrentService();
    }
}