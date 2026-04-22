using UnityEngine;

public class PlayerMovementUIBlocker : MonoBehaviour
{
    public static PlayerMovementUIBlocker Instance { get; private set; }

    [Header("Referências")]
    [SerializeField] private ThirdPersonCharacterMotor playerMotor;

    [Header("Busca automática")]
    [SerializeField] private bool findPlayerAutomatically = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    private int blockCount;
    private bool lastBlockedState;

    public bool IsBlocked => blockCount > 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        TryFindPlayerMotor();
        ApplyState();
    }

    public void AddBlock()
    {
        blockCount++;

        if (enableDebugLogs)
            Debug.Log($"[PlayerMovementUIBlocker] AddBlock | Total: {blockCount}");

        ApplyState();
    }

    public void RemoveBlock()
    {
        blockCount = Mathf.Max(0, blockCount - 1);

        if (enableDebugLogs)
            Debug.Log($"[PlayerMovementUIBlocker] RemoveBlock | Total: {blockCount}");

        ApplyState();
    }

    public void ClearAllBlocks()
    {
        blockCount = 0;

        if (enableDebugLogs)
            Debug.Log("[PlayerMovementUIBlocker] ClearAllBlocks");

        ApplyState();
    }

    private void ApplyState()
    {
        TryFindPlayerMotor();

        bool shouldBlock = IsBlocked;

        if (playerMotor != null && lastBlockedState != shouldBlock)
        {
            playerMotor.SetMovementEnabled(!shouldBlock);
            lastBlockedState = shouldBlock;
        }
        else if (playerMotor != null)
        {
            playerMotor.SetMovementEnabled(!shouldBlock);
        }
    }

    private void TryFindPlayerMotor()
    {
        if (playerMotor != null || !findPlayerAutomatically)
            return;

        GameObject playerObject = null;

        if (!string.IsNullOrWhiteSpace(playerTag))
            playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
            playerMotor = playerObject.GetComponent<ThirdPersonCharacterMotor>();

        if (playerMotor == null)
            playerMotor = FindFirstObjectByType<ThirdPersonCharacterMotor>();
    }
}