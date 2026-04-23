using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class ThirdPersonCharacterMotor : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Movimento")]
    [SerializeField] private float walkSpeed = 2.2f;
    [SerializeField] private float runSpeed = 4.8f;
    [SerializeField] private float sprintSpeed = 6.2f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 14f;
    [SerializeField] private float rotationSmoothTime = 0.10f;
    [SerializeField] private float inputDeadZone = 0.08f;

    [Header("Pulo e gravidade")]
    [SerializeField] private float jumpHeight = 1.25f;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float groundedGravity = -2f;
    [SerializeField] private float groundCheckRadius = 0.22f;

    [Header("Animação")]
    [SerializeField] private float animDamp = 0.12f;
    [SerializeField] private bool useStrafeMode = false;
    [SerializeField] private float directionalLerpSpeed = 10f;

    [Header("Estado")]
    [SerializeField] private bool movementEnabled = true;

    private CharacterController controller;
    private Animator animator;

    private Vector2 moveInput;
    private bool jumpPressed;
    private bool runHeld;
    private bool sprintHeld;

    private bool isGrounded;
    private float verticalVelocity;
    private float currentSpeed;
    private float rotationVelocity;

    private Vector3 lastMoveDirection = Vector3.forward;

    private float animMoveX;
    private float animMoveZ;

    private static readonly int GroundedHash = Animator.StringToHash("isGrounded");
    private static readonly int JumpHash = Animator.StringToHash("isJumping");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveZHash = Animator.StringToHash("MoveZ");
    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");

    public bool MovementEnabled => movementEnabled;
    public bool IsGrounded => isGrounded;
    public Vector2 MoveInput => moveInput;

    private bool CanUseController
    {
        get
        {
            return controller != null &&
                   controller.enabled &&
                   gameObject.activeInHierarchy &&
                   enabled;
        }
    }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform == null)
            return;

        if (animator == null)
            return;

        if (!CanUseController)
        {
            ResetMotionState();
            UpdateAnimatorSafe();
            return;
        }

        GroundCheck();
        HandleGravityAndJump();

        if (movementEnabled)
            HandleMovement();
        else
            StopMotion();

        UpdateAnimatorSafe();
    }

    public void SetMovementEnabled(bool value)
    {
        movementEnabled = value;

        if (!movementEnabled)
            ResetMotionState();
    }

    public void SetStrafeMode(bool value)
    {
        useStrafeMode = value;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!movementEnabled || !CanUseController)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = context.ReadValue<Vector2>();

        if (moveInput.magnitude < inputDeadZone)
            moveInput = Vector2.zero;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!movementEnabled || !CanUseController)
            return;

        if (context.performed)
            jumpPressed = true;
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (!movementEnabled || !CanUseController)
        {
            runHeld = false;
            return;
        }

        runHeld = context.ReadValueAsButton();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (!movementEnabled || !CanUseController)
        {
            sprintHeld = false;
            return;
        }

        sprintHeld = context.ReadValueAsButton();
    }

    private void ResetMotionState()
    {
        moveInput = Vector2.zero;
        jumpPressed = false;
        runHeld = false;
        sprintHeld = false;
        currentSpeed = 0f;
        verticalVelocity = 0f;
        animMoveX = 0f;
        animMoveZ = 0f;
    }

    private void GroundCheck()
    {
        if (!CanUseController)
        {
            isGrounded = false;
            return;
        }

        if (groundCheck == null)
        {
            isGrounded = controller.isGrounded;
        }
        else
        {
            isGrounded = Physics.CheckSphere(
                groundCheck.position,
                groundCheckRadius,
                groundLayer,
                QueryTriggerInteraction.Ignore
            );
        }

        if (animator != null)
            animator.SetBool(GroundedHash, isGrounded);
    }

    private void HandleGravityAndJump()
    {
        if (!CanUseController)
            return;

        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedGravity;

            if (animator != null)
                animator.SetBool(JumpHash, false);
        }

        if (jumpPressed && isGrounded && movementEnabled)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (animator != null)
                animator.SetBool(JumpHash, true);
        }

        jumpPressed = false;
        verticalVelocity += gravity * Time.deltaTime;
    }

    private void HandleMovement()
    {
        if (!CanUseController)
            return;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 desiredMoveDirection = camForward * moveInput.y + camRight * moveInput.x;
        float inputMagnitude = Mathf.Clamp01(moveInput.magnitude);

        bool hasMovementInput = inputMagnitude > 0.01f;

        if (hasMovementInput && desiredMoveDirection.sqrMagnitude > 0.0001f)
            lastMoveDirection = desiredMoveDirection.normalized;

        float targetMaxSpeed = walkSpeed;

        if (runHeld)
            targetMaxSpeed = runSpeed;

        if (sprintHeld)
            targetMaxSpeed = sprintSpeed;

        float targetSpeed = hasMovementInput ? targetMaxSpeed * inputMagnitude : 0f;

        float speedChangeRate = hasMovementInput ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedChangeRate * Time.deltaTime);

        Vector3 horizontalVelocity = Vector3.zero;

        if (desiredMoveDirection.sqrMagnitude > 0.0001f)
            horizontalVelocity = desiredMoveDirection.normalized * currentSpeed;

        Vector3 finalVelocity = horizontalVelocity + Vector3.up * verticalVelocity;

        controller.Move(finalVelocity * Time.deltaTime);

        HandleRotation(desiredMoveDirection, hasMovementInput);
        HandleDirectionalAnimation(desiredMoveDirection, hasMovementInput);
    }

    private void HandleRotation(Vector3 desiredMoveDirection, bool hasMovementInput)
    {
        if (!hasMovementInput)
            return;

        if (useStrafeMode)
        {
            Vector3 lookDirection = cameraTransform.forward;
            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude < 0.0001f)
                return;

            float targetAngle = Mathf.Atan2(lookDirection.x, lookDirection.z) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref rotationVelocity,
                rotationSmoothTime
            );

            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
        }
        else
        {
            if (desiredMoveDirection.sqrMagnitude < 0.0001f)
                return;

            Vector3 lookDirection = desiredMoveDirection.normalized;

            float targetAngle = Mathf.Atan2(lookDirection.x, lookDirection.z) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref rotationVelocity,
                rotationSmoothTime
            );

            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
        }
    }

    private void HandleDirectionalAnimation(Vector3 desiredMoveDirection, bool hasMovementInput)
    {
        Vector3 referenceDirection = desiredMoveDirection;

        Vector3 localDirection = transform.InverseTransformDirection(
            hasMovementInput && referenceDirection.sqrMagnitude > 0.0001f
                ? referenceDirection.normalized
                : Vector3.zero
        );

        float targetX = hasMovementInput ? Mathf.Clamp(localDirection.x, -1f, 1f) * moveInput.magnitude : 0f;
        float targetZ = hasMovementInput ? Mathf.Clamp(localDirection.z, -1f, 1f) * moveInput.magnitude : 0f;

        animMoveX = Mathf.Lerp(animMoveX, targetX, directionalLerpSpeed * Time.deltaTime);
        animMoveZ = Mathf.Lerp(animMoveZ, targetZ, directionalLerpSpeed * Time.deltaTime);
    }

    private void StopMotion()
    {
        if (!CanUseController)
        {
            ResetMotionState();
            return;
        }

        currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);

        Vector3 finalVelocity = Vector3.up * verticalVelocity;
        controller.Move(finalVelocity * Time.deltaTime);

        animMoveX = Mathf.Lerp(animMoveX, 0f, directionalLerpSpeed * Time.deltaTime);
        animMoveZ = Mathf.Lerp(animMoveZ, 0f, directionalLerpSpeed * Time.deltaTime);
    }

    private void UpdateAnimatorSafe()
    {
        if (animator == null)
            return;

        float horizontalSpeed = 0f;

        if (CanUseController)
            horizontalSpeed = new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;

        bool isMoving = horizontalSpeed > 0.08f && movementEnabled;

        animator.SetBool(IsMovingHash, isMoving);
        animator.SetFloat(SpeedHash, horizontalSpeed, animDamp, Time.deltaTime);
        animator.SetFloat(MoveXHash, animMoveX, animDamp, Time.deltaTime);
        animator.SetFloat(MoveZHash, animMoveZ, animDamp, Time.deltaTime);

        if (!CanUseController)
        {
            animator.SetBool(GroundedHash, false);
            animator.SetBool(JumpHash, false);
        }
    }

    private void OnDisable()
    {
        ResetMotionState();
    }

    private void OnDestroy()
    {
        ResetMotionState();
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}