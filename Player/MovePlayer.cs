using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class NewMonoBehaviourScript : MonoBehaviour
{
    private CharacterController characterController;
    private Animator animator;
    private Transform cameraMain;

    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 7f;
    [SerializeField] private float inputDeadZone = 0.1f;

    [Header("Chão")]
    [SerializeField] private Transform verificationPoint;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.15f;

    [Header("Pulo e gravidade")]
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -20f;

    [Header("Animação")]
    [SerializeField] private float animationDamp = 0.12f;
    [SerializeField] private bool useTurn180 = true;
    [SerializeField] private float turn180MinAngle = 150f;
    [SerializeField] private float turn180Cooldown = 0.7f;

    [Header("Estado")]
    [SerializeField] private bool movementEnabled = true;

    private bool isGrounded;
    private bool jumpRequested;
    private float verticalVelocity = -2f;
    private Vector2 inputPlayer;
    private float lastTurn180Time = -10f;

    private float currentAnimX;
    private float currentAnimZ;

    private static readonly int IsWalkingHash = Animator.StringToHash("isWalking");
    private static readonly int IsJumpingHash = Animator.StringToHash("isJumping");
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveZHash = Animator.StringToHash("MoveZ");
    private static readonly int Turn180Hash = Animator.StringToHash("Turn180");

    public bool MovementEnabled => movementEnabled;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (Camera.main != null)
            cameraMain = Camera.main.transform;
    }

    private void Update()
    {
        if (cameraMain == null)
        {
            if (Camera.main != null)
                cameraMain = Camera.main.transform;
            else
                return;
        }

        HandleGroundAndGravity();

        if (CanUseController())
        {
            if (movementEnabled)
                MoveCharacter();
            else
                StopMovementAnimation();
        }
        else
        {
            StopMovementAnimation();
        }

        UpdateAnimator();
    }

    public void SetMovementEnabled(bool value)
    {
        movementEnabled = value;

        if (!movementEnabled)
        {
            inputPlayer = Vector2.zero;
            jumpRequested = false;
            currentAnimX = 0f;
            currentAnimZ = 0f;

            if (animator != null)
                animator.SetBool(IsWalkingHash, false);
        }
    }

    public void MovePlayer(InputAction.CallbackContext context)
    {
        if (!movementEnabled)
        {
            inputPlayer = Vector2.zero;
            return;
        }

        inputPlayer = context.ReadValue<Vector2>();
    }

    public void JumpPlayer(InputAction.CallbackContext context)
    {
        if (!movementEnabled)
            return;

        if (context.performed)
            jumpRequested = true;
    }

    private void MoveCharacter()
    {
        if (!CanUseController())
            return;

        Vector3 cameraForward = cameraMain.forward;
        Vector3 cameraRight = cameraMain.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 worldDirection = (cameraForward * inputPlayer.y) + (cameraRight * inputPlayer.x);

        if (worldDirection.magnitude > 1f)
            worldDirection.Normalize();

        bool isMoving = worldDirection.magnitude > inputDeadZone;
        animator.SetBool(IsWalkingHash, isMoving);

        Vector3 localInputDirection = transform.InverseTransformDirection(worldDirection);

        currentAnimX = Mathf.Clamp(localInputDirection.x, -1f, 1f);
        currentAnimZ = Mathf.Clamp(localInputDirection.z, -1f, 1f);

        if (Mathf.Abs(currentAnimX) < 0.05f) currentAnimX = 0f;
        if (Mathf.Abs(currentAnimZ) < 0.05f) currentAnimZ = 0f;

        Vector3 horizontalMovement = worldDirection * moveSpeed;
        characterController.Move(horizontalMovement * Time.deltaTime);

        if (isMoving)
        {
            TryPlayTurn180(worldDirection);

            Quaternion targetRotation = Quaternion.LookRotation(worldDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void StopMovementAnimation()
    {
        if (animator != null)
            animator.SetBool(IsWalkingHash, false);

        currentAnimX = 0f;
        currentAnimZ = 0f;
    }

    private void HandleGroundAndGravity()
    {
        bool canUseController = CanUseController();

        if (verificationPoint != null)
        {
            isGrounded = Physics.CheckSphere(verificationPoint.position, groundCheckRadius, groundLayer);
        }
        else
        {
            isGrounded = false;
        }

        if (animator != null)
            animator.SetBool(IsGroundedHash, isGrounded);

        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;

            if (animator != null)
                animator.SetBool(IsJumpingHash, false);
        }

        if (jumpRequested && isGrounded && movementEnabled)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (animator != null)
                animator.SetBool(IsJumpingHash, true);

            jumpRequested = false;
        }

        verticalVelocity += gravity * Time.deltaTime;

        if (canUseController)
            characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    private void UpdateAnimator()
    {
        float speed = 0f;

        if (CanUseController())
        {
            Vector3 horizontalVelocity = characterController.velocity;
            horizontalVelocity.y = 0f;
            speed = movementEnabled ? horizontalVelocity.magnitude : 0f;
        }

        if (speed < 0.05f)
            speed = 0f;

        if (animator != null)
        {
            animator.SetFloat(MoveXHash, currentAnimX, animationDamp, Time.deltaTime);
            animator.SetFloat(MoveZHash, currentAnimZ, animationDamp, Time.deltaTime);
            animator.SetFloat(SpeedHash, speed, animationDamp, Time.deltaTime);
        }
    }

    private void TryPlayTurn180(Vector3 desiredDirection)
    {
        if (!useTurn180)
            return;

        if (Time.time < lastTurn180Time + turn180Cooldown)
            return;

        Vector3 currentForward = transform.forward;
        currentForward.y = 0f;
        currentForward.Normalize();

        Vector3 desired = desiredDirection;
        desired.y = 0f;
        desired.Normalize();

        if (desired.sqrMagnitude < 0.001f)
            return;

        float angle = Vector3.Angle(currentForward, desired);

        if (angle >= turn180MinAngle)
        {
            animator.SetTrigger(Turn180Hash);
            lastTurn180Time = Time.time;
        }
    }

    private bool CanUseController()
    {
        return characterController != null && characterController.enabled && gameObject.activeInHierarchy;
    }

    private void OnDrawGizmosSelected()
    {
        if (verificationPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(verificationPoint.position, groundCheckRadius);
        }
    }
}