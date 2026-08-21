using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 15f;
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private float lookSensitivity = 0.2f;
    [SerializeField] private float gravity = -19.62f;

    [SerializeField] private float rotationSpeed = 10f;
    private Vector3 moveDir;

    [Header("Camera")]
    [SerializeField] private CinemachineCamera cam;
    private Vector3 camForward;
    private Vector3 camRight;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float verticalVelocity;

    private CharacterController charController;
    private PlayerInputHandler inputHandler;
    private PlayerStamina stamina;

    void Awake()
    {
        charController = GetComponent<CharacterController>();
        inputHandler = GetComponent<PlayerInputHandler>();
        if (TryGetComponent(out PlayerStamina stamina))
            this.stamina = stamina;

        if (cam == null)
            cam = FindAnyObjectByType<CinemachineCamera>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnEnable()
    {
        inputHandler.OnMoveInput += HandleMoveInput;
        inputHandler.OnRunInput += HandleRunInput;
    }

    void OnDisable()
    {
        inputHandler.OnMoveInput -= HandleMoveInput;
        inputHandler.OnRunInput -= HandleRunInput;
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMoveInput(Vector2 input) => moveInput = input;

    private void HandleRunInput(bool isRunning)
    {
        if(stamina != null)
        {
            stamina.isRunning = isRunning;
        }
    } 

    private void HandleMovement()
    {
        camForward = cam.transform.forward;
        camRight = cam.transform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        moveDir = camForward * moveInput.y + camRight * moveInput.x;

        if (charController.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        bool isMoving = moveInput.sqrMagnitude > 0.1f;
        stamina.isMoving = isMoving;
        
        float currentSpeed = stamina.isRunning ? runSpeed : moveSpeed;

        Vector3 finalVelocity = (moveDir * currentSpeed) + (Vector3.up * verticalVelocity);
        charController.Move(finalVelocity * Time.deltaTime);
    }

    private void HandleRotation()
    {
        if(moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
