using System.Collections;
using UnityEngine;

public class PlayerExhaustionController : MonoBehaviour
{
    public static PlayerExhaustionController Instance { get; private set; }

    [Header("Referências")]
    [SerializeField] private Animator animator;
    [SerializeField] private NewMonoBehaviourScript playerMovement;
    [SerializeField] private CharacterController characterController;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer = ~0;
    [SerializeField] private float groundCheckHeight = 2f;
    [SerializeField] private float groundSnapOffset = 0.05f;

    [Header("Animação")]
    [SerializeField] private string collapseTriggerName = "Collapse";
    [SerializeField] private string standUpTriggerName = "StandUp";
    [SerializeField] private string exhaustedBoolName = "IsExhausted";
    [SerializeField] private float collapseAnimationLeadTime = 0.35f;
    [SerializeField] private float standUpAnimationDuration = 1.2f;

    [Header("Tempo no chão")]
    [SerializeField] private float minLayDownSeconds = 10f;
    [SerializeField] private float maxLayDownSeconds = 30f;

    [Header("Recuperação após colapso")]
    [SerializeField] private int minRecoveredMinutes = 20;
    [SerializeField] private int maxRecoveredMinutes = 45;
    [SerializeField] private float minRecoveredEnergy = 10f;
    [SerializeField] private float maxRecoveredEnergy = 20f;

    [Header("Root Motion")]
    [SerializeField] private bool disableRootMotionDuringCollapse = true;

    public bool IsInCollapseRoutine { get; private set; }

    private bool isBound;
    private Coroutine bindRoutine;
    private bool cachedApplyRootMotion;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Reset()
    {
        animator = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<NewMonoBehaviourScript>();
        characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        bindRoutine = StartCoroutine(BindRoutine());
    }

    private void OnDisable()
    {
        if (bindRoutine != null)
        {
            StopCoroutine(bindRoutine);
            bindRoutine = null;
        }

        Unbind();
    }

    private IEnumerator BindRoutine()
    {
        while (true)
        {
            if (!isBound && PlayerEnergySystem.Instance != null)
            {
                PlayerEnergySystem.Instance.OnEnergyDepleted += HandleEnergyDepleted;
                isBound = true;
                Debug.Log("[PlayerExhaustionController] Conectado ao PlayerEnergySystem.");
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    private void Unbind()
    {
        if (!isBound)
            return;

        if (PlayerEnergySystem.Instance != null)
            PlayerEnergySystem.Instance.OnEnergyDepleted -= HandleEnergyDepleted;

        isBound = false;
    }

    private void HandleEnergyDepleted()
    {
        if (IsInCollapseRoutine)
            return;

        StartCoroutine(CollapseRoutine());
    }

    private IEnumerator CollapseRoutine()
    {
        IsInCollapseRoutine = true;

        if (PlayerEnergySystem.Instance != null)
            PlayerEnergySystem.Instance.SetCollapsedState(true);

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(false);

        if (animator != null)
        {
            cachedApplyRootMotion = animator.applyRootMotion;

            if (disableRootMotionDuringCollapse)
                animator.applyRootMotion = false;

            if (HasBool(exhaustedBoolName))
                animator.SetBool(exhaustedBoolName, true);

            if (HasTrigger(collapseTriggerName))
            {
                animator.ResetTrigger(standUpTriggerName);
                animator.SetTrigger(collapseTriggerName);
            }
        }

        // Desliga o CharacterController para ele não "brigar" com a animação
        if (characterController != null)
            characterController.enabled = false;

        // Espera início da animação
        yield return new WaitForSeconds(collapseAnimationLeadTime);

        // Garante snap no chão depois de entrar na pose
        SnapToGround();

        float layDownTime = Random.Range(minLayDownSeconds, maxLayDownSeconds);
        yield return new WaitForSeconds(layDownTime);

        int addedMinutes = Random.Range(minRecoveredMinutes, maxRecoveredMinutes + 1);
        float recoveredEnergy = Random.Range(minRecoveredEnergy, maxRecoveredEnergy);

        if (GameTimeSystem.Instance != null)
            GameTimeSystem.Instance.AddMinutes(addedMinutes);

        // Antes de levantar, corrige novamente a posição no chão
        SnapToGround();

        if (animator != null)
        {
            if (HasBool(exhaustedBoolName))
                animator.SetBool(exhaustedBoolName, false);

            if (HasTrigger(standUpTriggerName))
            {
                animator.ResetTrigger(collapseTriggerName);
                animator.SetTrigger(standUpTriggerName);
            }
        }

        if (PlayerEnergySystem.Instance != null)
            PlayerEnergySystem.Instance.ApplyCollapseRecovery(recoveredEnergy);

        yield return new WaitForSeconds(standUpAnimationDuration);

        // Corrige no chão antes de religar o controller
        SnapToGround();

        if (characterController != null)
            characterController.enabled = true;

        if (animator != null)
            animator.applyRootMotion = cachedApplyRootMotion;

        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);

        IsInCollapseRoutine = false;
    }

    private void SnapToGround()
    {
        Vector3 origin = transform.position + Vector3.up * groundCheckHeight;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundCheckHeight + 5f, groundLayer))
        {
            Vector3 newPos = transform.position;
            newPos.y = hit.point.y + groundSnapOffset;
            transform.position = newPos;
        }
    }

    private bool HasTrigger(string parameterName)
    {
        if (animator == null || string.IsNullOrWhiteSpace(parameterName))
            return false;

        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            if (p.name == parameterName && p.type == AnimatorControllerParameterType.Trigger)
                return true;
        }

        return false;
    }

    private bool HasBool(string parameterName)
    {
        if (animator == null || string.IsNullOrWhiteSpace(parameterName))
            return false;

        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            if (p.name == parameterName && p.type == AnimatorControllerParameterType.Bool)
                return true;
        }

        return false;
    }
}