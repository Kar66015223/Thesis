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
    [SerializeField] private float gravity = -9.81f;

    [SerializeField] private float rotationSpeed = 10f;
    private Vector3 moveDir;

    private bool canMove = true;

    [Header("Camera")]
    [SerializeField] private CinemachineCamera cam;
    private Vector3 camForward;
    private Vector3 camRight;

    [SerializeField] private Transform playerModel;

    [Header("Camera Target Positioning")]
    [SerializeField] private Transform camTarget;
    [SerializeField] private float shoulderOffset = 1f;
    [SerializeField] private float targetHeight = 1f;

    private Vector2 moveInput;
    private float verticalVelocity;

    private CharacterController charController;
    private PlayerInputHandler inputHandler;

    private PlayerStamina stamina;
    private PlayerInteraction interaction;
    private PlayerScannerSkill scanSkill;

    [SerializeField] private PlayerAnimation anim = new();

    void Awake()
    {
        charController = GetComponent<CharacterController>();
        inputHandler = GetComponent<PlayerInputHandler>();
        if (cam == null)
            cam = FindAnyObjectByType<CinemachineCamera>();
        if (playerModel == null)
            playerModel = GetComponentInChildren<Animator>().transform;

        if (TryGetComponent(out PlayerStamina stamina))
            this.stamina = stamina;
        interaction = GetComponentInChildren<PlayerInteraction>();
        scanSkill = GetComponentInChildren<PlayerScannerSkill>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnEnable()
    {
        inputHandler.OnMoveInput += HandleMoveInput;
        inputHandler.OnRunInput += HandleRunInput;
        inputHandler.OnInteractInput += HandleInteractInput;

        inputHandler.OnUseDemonEyeSkillInput += HandleDemonEyeSkillUsage;
    }

    void OnDisable()
    {
        inputHandler.OnMoveInput -= HandleMoveInput;
        inputHandler.OnRunInput -= HandleRunInput;
        inputHandler.OnInteractInput -= HandleInteractInput;

        inputHandler.OnUseDemonEyeSkillInput -= HandleDemonEyeSkillUsage;
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        UpdateCameraTarget();
    }

    private void HandleMoveInput(Vector2 input) => moveInput = input;

    private void HandleRunInput(bool isRunning)
    {
        if (stamina != null)
        {
            stamina.isRunning = isRunning;
        }
    }
    
    private void HandleInteractInput()
    {
        interaction.handler.PerformInteract();
        anim.SetInteract();
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

        if (canMove)
        {
            bool isMoving = moveInput.sqrMagnitude > 0.1f;
            stamina.isMoving = isMoving;
    
            float currentSpeed = stamina.isRunning ? runSpeed : moveSpeed;
    
            Vector3 finalVelocity = (moveDir * currentSpeed) + (Vector3.up * verticalVelocity);
            charController.Move(finalVelocity * Time.deltaTime);
    
            if (isMoving)
                anim.SetMove(1);
            else
                anim.SetMove(0);
        }
    }

    private void HandleRotation()
    {
        if (!canMove)
            return;
            
        if (moveDir != Vector3.zero && playerModel != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);

            playerModel.rotation = Quaternion.Slerp(
                playerModel.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void UpdateCameraTarget()
    {
        if (camTarget == null || cam == null)
            return;

        Vector3 right = cam.transform.right;
        right.y = 0;
        right.Normalize();

        camTarget.position = transform.position + (Vector3.up * targetHeight) + (right * shoulderOffset);
    }

    private void HandleDemonEyeSkillUsage()
    {
        scanSkill.Scan();
    }

    public void SetCanMove(bool flag) => canMove = flag;
}
