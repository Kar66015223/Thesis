using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerState state = PlayerState.Normal;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 15f;
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private float lookSensitivity = 0.2f;
    [SerializeField] private float gravity = -9.81f;

    [SerializeField] private float rotationSpeed = 10f;
    private Vector3 moveDir;

    private Vector2 moveInput;
    private float verticalVelocity;

    [Header("States")]
    private bool canMove = true;
    private bool isMoving;
    public bool IsCrouching { get; private set; } = false;
    private bool canInteract;
    private bool isUsingSkill;

    private bool isHiding;
    private HidingPlace curHidingPlace;

    [Header("Other")]
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

        inputHandler.OnDemonEyeSkillInput += HandleDemonEyeSkillUsage;
    }

    void OnDisable()
    {
        inputHandler.OnMoveInput -= HandleMoveInput;
        inputHandler.OnRunInput -= HandleRunInput;
        inputHandler.OnCrouchInput -= HandleCrouchInput;
        inputHandler.OnInteractInput -= HandleInteractInput;

        inputHandler.OnDemonEyeSkillInput -= HandleDemonEyeSkillUsage;
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        UpdatePlayerState();
        camController.Update();
    }

    private void UpdatePlayerState()
    {
        if (IsCrouching)
            ChangeState(PlayerState.Crouching);

        else if (stamina.isRunning)
            ChangeState(PlayerState.Running);

        else if (isUsingSkill)
            ChangeState(PlayerState.UsingSkill);

        else if (isHiding)
            ChangeState(PlayerState.Hiding);

        else
            ChangeState(PlayerState.Normal);
    }

    private void HandleMoveInput(Vector2 input) 
        => moveInput = input;

    private void HandleRunInput(bool isRunning)
    {
        if (state == PlayerState.Crouching || state == PlayerState.UsingSkill)
            return;
            
        if (stamina != null)
        {
            stamina.isRunning = isRunning;
            camController.isRunning = isRunning;
        }
    }

    private void HandleCrouchInput(bool input)
    {
        if (state == PlayerState.Running)
            return;

        IsCrouching = input;

        anim.SetCrouch(IsCrouching);
        camController.ChangeCamHeight(IsCrouching);
        camController.isCrouching = IsCrouching;
    }

    private void HandleInteractInput()
    {
        if (state == PlayerState.Hiding)
        {
            curHidingPlace.ToggleHide(gameObject);
            return;
        }

        canInteract = !(state == PlayerState.Running) && interaction.detector.GetAllDetected().Count > 0;
        if (!canInteract)
            return;

        interaction.handler.PerformInteract();
        anim.SetInteract();
    }
    
    private void HandleDemonEyeSkillUsage(bool flag)
    {
        if (state == PlayerState.Running)
            return;
        if (flag && scanSkill.isCooldown)
            return;

        scanSkill.Scan(flag);
        camController.isDemonEyeActive = flag;
        isUsingSkill = flag;
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
            if(charController.enabled)
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

    public void SetCanMove(bool flag)
        => canMove = flag;

    public void SetHiding(bool flag, HidingPlace place, Vector3 targetPos)
    {
        isHiding = flag;
        curHidingPlace = place;

        charController.enabled = false;
        transform.position = targetPos;

        charController.enabled = !flag;
        playerModel.gameObject.SetActive(!flag);
    }

    public void ChangeState(PlayerState newState)
        => state = newState;
}
