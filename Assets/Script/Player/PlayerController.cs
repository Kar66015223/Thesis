using UnityEngine;

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
    private bool isMoving;

    public bool IsCrouching { get; private set; } = false;

    private bool canCrouch;
    private bool canRun;
    private bool canInteract;

    private Vector2 moveInput;
    private float verticalVelocity;

    [SerializeField] private Transform playerModel;

    private CharacterController charController;
    private PlayerInputHandler inputHandler;
    [SerializeField] private PlayerCameraController camController = new();

    private PlayerStamina stamina;
    private PlayerInteraction interaction;
    private PlayerScannerSkill scanSkill;

    [SerializeField] private PlayerAnimation anim = new();

    void Awake()
    {
        charController = GetComponent<CharacterController>();
        inputHandler = GetComponent<PlayerInputHandler>();
        camController.Initialize(this);
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
        inputHandler.OnCrouchInput += HandleCrouchInput;
        inputHandler.OnInteractInput += HandleInteractInput;

        inputHandler.OnHoldDemonEyeSkillInput += HandleDemonEyeSkillUsage;
    }

    void OnDisable()
    {
        inputHandler.OnMoveInput -= HandleMoveInput;
        inputHandler.OnRunInput -= HandleRunInput;
        inputHandler.OnCrouchInput -= HandleCrouchInput;
        inputHandler.OnInteractInput -= HandleInteractInput;

        inputHandler.OnHoldDemonEyeSkillInput -= HandleDemonEyeSkillUsage;
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        
        camController.UpdateCameraTarget();
        camController.UpdateFOVChange();
    }

    private void HandleMoveInput(Vector2 input) => moveInput = input;

    private void HandleRunInput(bool isRunning)
    {
        canRun = !IsCrouching;
        if (!canRun)
            return;
            
        if (stamina != null)
        {
            stamina.isRunning = isRunning;
        }
    }

    private void HandleCrouchInput(bool input)
    {
        canCrouch = !stamina.isRunning;
        if (!canCrouch)
            return;
        
        IsCrouching = input;

        anim.SetCrouch(IsCrouching);
        camController.ChangeCamHeight(IsCrouching);
    }

    private void HandleInteractInput()
    {
        canInteract = !stamina.isRunning && interaction.detector.GetAllDetected().Count > 0;
        if (!canInteract)
            return;

        interaction.handler.PerformInteract();
        anim.SetInteract();
    }
    
    private void HandleDemonEyeSkillUsage(bool flag)
    {
        scanSkill.Scan(flag);
        camController.ChangeFOV(flag, camController.scanSkillFOVChange);
    }

    private void HandleMovement()
    {
        camController.camForward = camController.cam.transform.forward;
        camController.camRight = camController.cam.transform.right;

        camController.camForward.y = 0;
        camController.camRight.y = 0;
        camController.camForward.Normalize();
        camController.camRight.Normalize();

        moveDir = camController.camForward * moveInput.y + camController.camRight * moveInput.x;

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
            isMoving = moveInput.sqrMagnitude > 0.1f;
            stamina.isMoving = isMoving;

            float currentSpeed;

            if (!IsCrouching)
            {
                currentSpeed = stamina.isRunning ? runSpeed : moveSpeed;
            }
            else
            {
                currentSpeed = crouchSpeed;
            }

            Vector3 finalVelocity = (moveDir * currentSpeed) + (Vector3.up * verticalVelocity);
            charController.Move(finalVelocity * Time.deltaTime);

            UpdateMovementAnim();
        }
    }
    
    private void UpdateMovementAnim()
    {
        int animMovementValue = 0;

        
        if (isMoving)
        {
            if (IsCrouching)
            {
                animMovementValue = 1;
            }
            else
            {
                animMovementValue = stamina.isRunning ? 2 : 1;
            }
        }

        anim.SetMove(animMovementValue);
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

    public void SetCanMove(bool flag) => canMove = flag;
}
