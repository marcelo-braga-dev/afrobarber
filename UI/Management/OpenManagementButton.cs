using UnityEngine;

public class OpenManagementButton : MonoBehaviour
{
    [SerializeField] private BarbershopManagementUI managementUI;

    public void OpenManagement()
    {
        if (managementUI != null)
        {
            managementUI.Open();
            return;
        }

        if (GameUIManager.Instance != null)
            GameUIManager.Instance.OpenGestao();
    }

    public void CloseManagement()
    {
        if (managementUI != null)
        {
            managementUI.Close();
            return;
        }

        if (GameUIManager.Instance != null)
            GameUIManager.Instance.CloseGestao();
    }
}