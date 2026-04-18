using UnityEngine;
using UnityEngine.UI;

public class CarRestPoint : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Button goHomeButton;
    [SerializeField] private RestUIController restUIController;

    [Header("Configuração")]
    [SerializeField] private float interactionDistance = 3f;

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        if (goHomeButton != null)
        {
            goHomeButton.gameObject.SetActive(false);
            goHomeButton.onClick.RemoveAllListeners();
            goHomeButton.onClick.AddListener(OnClickGoHome);
        }
    }

    private void Update()
    {
        if (playerTransform == null || goHomeButton == null)
            return;

        float distance = Vector3.Distance(playerTransform.position, transform.position);
        bool isCloseEnough = distance <= interactionDistance;

        bool blockedByCollapse = PlayerExhaustionController.Instance != null &&
                                 PlayerExhaustionController.Instance.IsInCollapseRoutine;

        bool blockedByBusinessHours = IsDuringBusinessHours();

        bool canShow = isCloseEnough && !blockedByCollapse && !blockedByBusinessHours;

        goHomeButton.gameObject.SetActive(canShow);
    }

    private void OnClickGoHome()
    {
        if (IsDuringBusinessHours())
        {
            Debug.Log("[CarRestPoint] Descanso bloqueado: ainda está no horário de funcionamento.");
            return;
        }

        if (restUIController == null)
        {
            Debug.LogWarning("[CarRestPoint] RestUIController não configurado.");
            return;
        }

        restUIController.StartRestSequence();
    }

    private bool IsDuringBusinessHours()
    {
        if (GameTimeSystem.Instance == null)
            return false;

        return GameTimeSystem.Instance.IsWorkDay && GameTimeSystem.Instance.IsWithinBusinessHours;
    }
}