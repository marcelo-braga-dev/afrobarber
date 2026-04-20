using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInteractionController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float interactDistance = 10f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private LayerMask interactionLayerMask = ~0;
    [SerializeField] private bool enableDebugLogs = true;

    [Header("Bloqueio por UI")]
    [SerializeField] private bool blockWhenAnyBlockingPanelIsOpen = true;

    [Tooltip("Painéis que realmente bloqueiam interação com NPC. Ex: Loja, Inventário, Financeiro, Configurações, Atendimento.")]
    [SerializeField] private GameObject[] blockingUIPanels;

    [Header("Bloqueio por clique em UI")]
    [SerializeField] private bool blockPointerOverUIOnlyWhenPanelIsOpen = true;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(interactKey))
            TryInteract();
    }

    public void TryInteract()
    {
        if (ShouldBlockInteraction())
            return;

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

    private bool ShouldBlockInteraction()
    {
        bool hasBlockingPanelOpen = IsAnyBlockingUIPanelOpen();

        if (blockWhenAnyBlockingPanelIsOpen && hasBlockingPanelOpen)
        {
            if (enableDebugLogs)
                Debug.Log("[PlayerInteractionController] Interação bloqueada: existe UI bloqueante aberta.");

            return true;
        }

        if (blockPointerOverUIOnlyWhenPanelIsOpen && hasBlockingPanelOpen && IsPointerOverUI())
        {
            if (enableDebugLogs)
                Debug.Log("[PlayerInteractionController] Interação bloqueada: ponteiro está sobre UI bloqueante.");

            return true;
        }

        return false;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
#endif

        return EventSystem.current.IsPointerOverGameObject();
    }

    private bool IsAnyBlockingUIPanelOpen()
    {
        if (blockingUIPanels == null || blockingUIPanels.Length == 0)
            return false;

        for (int i = 0; i < blockingUIPanels.Length; i++)
        {
            GameObject panel = blockingUIPanels[i];

            if (panel != null && panel.activeInHierarchy)
                return true;
        }

        return false;
    }
}