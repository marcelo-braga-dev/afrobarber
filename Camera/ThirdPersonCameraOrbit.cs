using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCameraOrbit : MonoBehaviour
{
    [Header("Alvo")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.6f, 0f);

    [Header("Distância")]
    [SerializeField] private float distance = 4.5f;
    [SerializeField] private float minDistance = 2.2f;
    // [SerializeField] private float maxDistance = 6f;

    [Header("Rotação")]
    [SerializeField] private float mouseSensitivity = 130f;
    [SerializeField] private float gamepadSensitivity = 220f;
    [SerializeField] private float minPitch = -25f;
    [SerializeField] private float maxPitch = 65f;
    [SerializeField] private bool invertY = false;

    [Header("Suavização")]
    [SerializeField] private float positionSmoothTime = 0.06f;
    [SerializeField] private float rotationSmooth = 15f;

    [Header("Colisão")]
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private float cameraCollisionRadius = 0.2f;
    [SerializeField] private float collisionBuffer = 0.15f;

    private Vector2 lookInput;
    private Vector3 currentVelocity;

    private float yaw;
    private float pitch;

    private void Start()
    {
        if (target == null)
            return;

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        UpdateRotation();
        UpdatePosition();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void UpdateRotation()
    {
        bool usingMouse = Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.0001f;
        float sensitivity = usingMouse ? mouseSensitivity : gamepadSensitivity;

        float yInput = invertY ? lookInput.y : -lookInput.y;

        yaw += lookInput.x * sensitivity * Time.deltaTime;
        pitch += yInput * sensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private void UpdatePosition()
    {
        Vector3 focusPoint = target.position + targetOffset;

        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredDirection = targetRotation * Vector3.back;
        float correctedDistance = distance;

        if (Physics.SphereCast(
                focusPoint,
                cameraCollisionRadius,
                desiredDirection,
                out RaycastHit hit,
                distance,
                collisionLayers,
                QueryTriggerInteraction.Ignore))
        {
            correctedDistance = Mathf.Clamp(hit.distance - collisionBuffer, minDistance, distance);
        }

        Vector3 desiredPosition = focusPoint + desiredDirection * correctedDistance;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            positionSmoothTime
        );

        Quaternion finalRotation = Quaternion.LookRotation(focusPoint - transform.position);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            finalRotation,
            rotationSmooth * Time.deltaTime
        );
    }
}