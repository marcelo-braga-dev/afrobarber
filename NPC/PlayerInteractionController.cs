using UnityEngine;

public class PlayerInteractionController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float interactDistance = 10f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private LayerMask interactionLayerMask = ~0;
    [SerializeField] private bool enableDebugLogs = true;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    public void TryInteract()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogWarning("[PlayerInteractionController] mainCamera não encontrada.");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactionLayerMask))
        {
            if (enableDebugLogs)
                Debug.Log($"[PlayerInteractionController] Raycast acertou: {hit.collider.name}");

            ClientNPC client = hit.collider.GetComponentInParent<ClientNPC>();

            if (client != null)
            {
                if (enableDebugLogs)
                    Debug.Log($"[PlayerInteractionController] ClientNPC encontrado: {client.name}");

                client.OnPlayerClicked();
                return;
            }

            if (enableDebugLogs)
                Debug.LogWarning("[PlayerInteractionController] O collider clicado não pertence a um ClientNPC.");
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log("[PlayerInteractionController] Raycast não acertou nada.");
        }
    }
}