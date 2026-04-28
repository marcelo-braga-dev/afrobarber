using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerInteractionController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float interactDistance = 10f;
    [SerializeField] private LayerMask interactionLayerMask = ~0;
    [SerializeField] private bool enableDebugLogs = true;

    [Header("Entrada")]
    [SerializeField] private Key interactKey = Key.E;
    [SerializeField] private bool allowMouseClick = true;
    [SerializeField] private bool allowTouch = true;

    [Header("Bloqueio por UI")]
    [SerializeField] private bool blockWhenAnyBlockingPanelIsOpen = true;

    [Tooltip("Painéis que realmente bloqueiam interação com NPC. Ex: Loja, Inventário, Financeiro, Configurações, Atendimento.")]
    [SerializeField] private GameObject[] blockingUIPanels;

    [Header("Bloqueio por clique em UI")]
    [SerializeField] private bool blockPointerOverUIOnlyWhenPanelIsOpen = true;

    private Vector2 lastPointerPosition;

    private void Update()
    {
        bool interactionRequested = false;

        if (Keyboard.current != null && Keyboard.current[interactKey].wasPressedThisFrame)
        {
            interactionRequested = true;
            lastPointerPosition = GetScreenCenter();
        }

        if (allowMouseClick && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            interactionRequested = true;
            lastPointerPosition = Mouse.current.position.ReadValue();
        }

        if (allowTouch && Touchscreen.current != null)
        {
            foreach (TouchControl touch in Touchscreen.current.touches)
            {
                if (touch.press.wasPressedThisFrame)
                {
                    interactionRequested = true;
                    lastPointerPosition = touch.position.ReadValue();
                    break;
                }
            }
        }

        if (interactionRequested)
            TryInteract(lastPointerPosition);
    }

    public void TryInteract()
    {
        TryInteract(GetCurrentPointerPosition());
    }

    public void TryInteract(Vector2 screenPosition)
    {
        if (ShouldBlockInteraction(screenPosition))
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogWarning("[PlayerInteractionController] mainCamera não encontrada.");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

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

    private bool ShouldBlockInteraction(Vector2 screenPosition)
    {
        bool hasBlockingPanelOpen = IsAnyBlockingUIPanelOpen();

        if (blockWhenAnyBlockingPanelIsOpen && hasBlockingPanelOpen)
        {
            if (enableDebugLogs)
                Debug.Log("[PlayerInteractionController] Interação bloqueada: existe UI bloqueante aberta.");

            return true;
        }

        if (blockPointerOverUIOnlyWhenPanelIsOpen && hasBlockingPanelOpen && IsPointerOverUI(screenPosition))
        {
            if (enableDebugLogs)
                Debug.Log("[PlayerInteractionController] Interação bloqueada: ponteiro está sobre UI bloqueante.");

            return true;
        }

        return false;
    }

    private bool IsPointerOverUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return results.Count > 0;
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

    private Vector2 GetCurrentPointerPosition()
    {
        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();

        if (Touchscreen.current != null)
        {
            foreach (TouchControl touch in Touchscreen.current.touches)
            {
                if (touch.press.isPressed)
                    return touch.position.ReadValue();
            }
        }

        return GetScreenCenter();
    }

    private Vector2 GetScreenCenter()
    {
        return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }
}