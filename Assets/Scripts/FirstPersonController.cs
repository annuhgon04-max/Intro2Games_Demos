// First-person movement example written with the support of GitHub Copilot.
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    private enum MovementMode { Grounded, Flying }

    [Header("References")]
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference crouchAction;

    [Header("Movement Mode")]
    [SerializeField] private MovementMode movementMode = MovementMode.Grounded;
    [SerializeField] private float flyingSpeed = 8f;
    [SerializeField] private float verticalMovementSpeed = 4f;
    [SerializeField] private bool useCameraDirectionForFlight = true;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float acceleration = 18f;
    [SerializeField] private float deceleration = 24f;
    [SerializeField] private bool allowAirControl = true;
    [SerializeField] private float airControl = 0.5f;

    [Header("Jump and Gravity")]
    [SerializeField] private bool canJump = true;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedStickForce = -2f;

    [Header("Crouch")]
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private float crouchHeight = 1.1f;
    [SerializeField] private float crouchSpeedMultiplier = 0.55f;
    [SerializeField] private float crouchTransitionSpeed = 8f;

    [Header("Look")]
    [SerializeField] private float lookSensitivity = 0.1f;
    [SerializeField] private bool invertLook;
    [SerializeField] private float minimumPitch = -85f;
    [SerializeField] private float maximumPitch = 85f;

    private CharacterController characterController;
    private float verticalVelocity;
    private float currentSpeed;
    private float pitch;
    private float standingHeight;
    private Vector3 standingCenter;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        standingHeight = characterController.height;
        standingCenter = characterController.center;
    }

    private void OnEnable()
    {
        EnableAction(moveAction);
        EnableAction(lookAction);
        EnableAction(jumpAction);
        EnableAction(sprintAction);
        EnableAction(crouchAction);
    }

    private void OnDisable()
    {
        DisableAction(moveAction);
        DisableAction(lookAction);
        DisableAction(jumpAction);
        DisableAction(sprintAction);
        DisableAction(crouchAction);
    }

    private void Update()
    {
        HandleLook();
        HandleCrouch();
        HandleMovement();
    }

    private void HandleLook()
    {
        if (cameraRoot == null || lookAction == null) return;
        Vector2 look = lookAction.action.ReadValue<Vector2>() * lookSensitivity;
        pitch = Mathf.Clamp(pitch + (invertLook ? look.y : -look.y), minimumPitch, maximumPitch);
        transform.Rotate(Vector3.up * look.x);
        cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleMovement()
    {
        Vector2 input = moveAction == null ? Vector2.zero : moveAction.action.ReadValue<Vector2>();
        if (movementMode == MovementMode.Flying)
        {
            HandleFreeMovement(input);
            return;
        }
        Vector3 direction = transform.right * input.x + transform.forward * input.y;
        bool sprinting = sprintAction != null && sprintAction.action.IsPressed();
        bool crouching = canCrouch && crouchAction != null && crouchAction.action.IsPressed();
        float targetSpeed = (sprinting ? sprintSpeed : walkSpeed) * (crouching ? crouchSpeedMultiplier : 1f);
        float control = characterController.isGrounded ? 1f : (allowAirControl ? airControl : 0f);
        currentSpeed = Mathf.MoveTowards(currentSpeed, direction.sqrMagnitude > 0.01f ? targetSpeed : 0f,
            (direction.sqrMagnitude > 0.01f ? acceleration : deceleration) * control * Time.deltaTime);
        if (characterController.isGrounded)
        {
            verticalVelocity = groundedStickForce;
            if (canJump && jumpAction != null && jumpAction.action.WasPressedThisFrame())
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        verticalVelocity += gravity * Time.deltaTime;
        Vector3 velocity = direction.normalized * currentSpeed;
        velocity.y = verticalVelocity;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleFreeMovement(Vector2 input)
    {
        bool ascending = jumpAction != null && jumpAction.action.IsPressed();
        bool descending = crouchAction != null && crouchAction.action.IsPressed();
        float verticalInput = (ascending ? 1f : 0f) - (descending ? 1f : 0f);
        Vector3 forward = useCameraDirectionForFlight && cameraRoot != null
            ? cameraRoot.forward : transform.forward;
        float speed = flyingSpeed;
        Vector3 horizontalDirection = forward * input.y + transform.right * input.x;
        Vector3 direction = horizontalDirection.normalized * speed;
        direction.y += verticalInput * verticalMovementSpeed;
        characterController.Move(direction * Time.deltaTime);
    }

    private void HandleCrouch()
    {
        if (!canCrouch || crouchAction == null) return;
        float targetHeight = crouchAction.action.IsPressed() ? crouchHeight : standingHeight;
        characterController.height = Mathf.MoveTowards(characterController.height, targetHeight,
            crouchTransitionSpeed * Time.deltaTime);
        characterController.center = standingCenter + Vector3.down * (standingHeight - characterController.height) * 0.5f;
    }

    private static void EnableAction(InputActionReference actionReference)
    {
        if (actionReference != null) actionReference.action.Enable();
    }

    private static void DisableAction(InputActionReference actionReference)
    {
        if (actionReference != null) actionReference.action.Disable();
    }
}