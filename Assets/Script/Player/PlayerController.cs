using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Composition root for the player's body: turns movement input into motion and owns the
/// movement state (crouch / run / hiding / using skill). The actual work is delegated to
/// <see cref="PlayerMotor"/>, <see cref="PlayerCameraController"/> and <see cref="PlayerAnimation"/>.
/// Interaction, skills, stamina and health live in their own components.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerController : MonoBehaviour, IInteractorBody, IHidingAgent, IItemHolder, ISkillUser
{
    private const int InventoryCapacity = 30;
    private const float MoveInputDeadZoneSqr = 0.1f;

    [field: SerializeField] public PlayerState CurrentState { get; private set; } = PlayerState.Normal;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 15f;
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Collider Size")]
    [SerializeField] private float standColliderY = 0.7f;
    [SerializeField] private float standColliderHeight = 1.3f;
    [SerializeField] private float crouchColliderY = 0.5f;
    [SerializeField] private float crouchColliderHeight = 0.8f;

    [Header("Other")]
    [SerializeField] private Transform playerModel;
    [SerializeField] private PlayerCameraController camController = new();
    [SerializeField] private PlayerAnimation anim = new();

    private CharacterController charController;
    private PlayerInputHandler inputHandler;
    private PlayerStamina stamina;
    private PlayerHealth health;
    private PlayerMotor motor;
    private readonly List<Renderer> hiddenRenderers = new();

    private Vector2 moveInput;
    private Vector3 moveDir;
    private bool isRunHeld;
    private bool canMove = true;
    private bool isUsingSkill;

    public bool IsCrouching { get; private set; }
    public bool IsRunning => stamina != null && stamina.IsRunning;
    public bool IsHiding => CurrentHidingPlace != null;
    public Hidable CurrentHidingPlace { get; private set; }

    /// <summary>Running, using a skill or hiding all block starting a new interaction.</summary>
    public bool CanStartInteraction =>
        CurrentState == PlayerState.Normal || CurrentState == PlayerState.Crouching;

    /// <summary>
    /// Collected items. Lives here only until the inventory gets its own component on the Player prefab;
    /// other code talks to it through <see cref="IItemHolder"/>, so moving it later is a one-line change.
    /// </summary>
    public InventoryModel Inventory { get; } = new(InventoryCapacity);

    void Awake()
    {
        charController = GetComponent<CharacterController>();
        inputHandler = GetComponent<PlayerInputHandler>();
        stamina = GetComponent<PlayerStamina>();
        health = GetComponent<PlayerHealth>();
        motor = new PlayerMotor(charController);

        if (playerModel == null)
        {
            Animator animator = GetComponentInChildren<Animator>();
            if (animator != null)
                playerModel = animator.transform;
        }

        camController.Initialize(transform);
        ApplyStance();
    }

    void OnEnable()
    {
        inputHandler.OnMoveInput += HandleMoveInput;
        inputHandler.OnRunInput += HandleRunInput;
        inputHandler.OnCrouchPressed += HandleCrouchPressed;

        if (health != null)
        {
            health.Died += HandleDied;
            health.Revived += HandleRevived;
        }
    }

    void OnDisable()
    {
        inputHandler.OnMoveInput -= HandleMoveInput;
        inputHandler.OnRunInput -= HandleRunInput;
        inputHandler.OnCrouchPressed -= HandleCrouchPressed;

        if (health != null)
        {
            health.Died -= HandleDied;
            health.Revived -= HandleRevived;
        }

        // Releases arrive while we're unsubscribed, so don't keep walking on stale input.
        moveInput = Vector2.zero;
        isRunHeld = false;
    }

    void Update()
    {
        UpdateMovement();
        UpdateModelRotation();
        UpdateState();

        bool isTired = stamina != null && stamina.IsExhausted;
        camController.Tick(new CameraViewState(IsRunning, IsCrouching, isUsingSkill, isTired));
    }

    #region Input

    private void HandleMoveInput(Vector2 input) => moveInput = input;

    private void HandleRunInput(bool isHeld) => isRunHeld = isHeld;

    private void HandleCrouchPressed()
    {
        if (CurrentState == PlayerState.Running || IsHiding)
            return;

        IsCrouching = !IsCrouching;
        anim.SetCrouch(IsCrouching);
        ApplyStance();
    }

    private void HandleDied() => SetCanMove(false);
    private void HandleRevived() => SetCanMove(true);

    #endregion

    #region Movement

    private void UpdateMovement()
    {
        camController.GetPlanarBasis(out Vector3 forward, out Vector3 right);
        moveDir = forward * moveInput.y + right * moveInput.x;

        if (!canMove)
        {
            UpdateStamina(isMoving: false);
            return;
        }

        bool isMoving = moveInput.sqrMagnitude > MoveInputDeadZoneSqr;
        UpdateStamina(isMoving);

        motor.Move(moveDir * GetCurrentSpeed(), gravity, Time.deltaTime);
        anim.SetMove(GetMoveAnimValue(isMoving));
    }

    private void UpdateStamina(bool isMoving)
    {
        if (stamina == null)
            return;

        stamina.SetMoving(isMoving);
        stamina.SetRunIntent(isRunHeld && !IsCrouching && !isUsingSkill);
    }

    private float GetCurrentSpeed()
    {
        if (IsCrouching)
            return crouchSpeed;

        return IsRunning ? runSpeed : moveSpeed;
    }

    private int GetMoveAnimValue(bool isMoving)
    {
        if (!isMoving)
            return 0;

        return !IsCrouching && IsRunning ? 2 : 1;
    }

    private void UpdateModelRotation()
    {
        if (!canMove || playerModel == null || moveDir == Vector3.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(moveDir);
        playerModel.rotation = Quaternion.Slerp(playerModel.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void ApplyStance()
    {
        if (IsCrouching)
            motor.SetStance(crouchColliderY, crouchColliderHeight);
        else
            motor.SetStance(standColliderY, standColliderHeight);
    }

    /// <summary>Highest priority first: hiding beats everything, crouching is the weakest.</summary>
    private void UpdateState()
    {
        if (IsHiding)
            CurrentState = PlayerState.Hiding;
        else if (isUsingSkill)
            CurrentState = PlayerState.UsingSkill;
        else if (IsRunning)
            CurrentState = PlayerState.Running;
        else if (IsCrouching)
            CurrentState = PlayerState.Crouching;
        else
            CurrentState = PlayerState.Normal;
    }

    #endregion

    #region Public API

    public void SetUsingSkill(bool isActive) => isUsingSkill = isActive;

    public void PlayInteractAnimation() => anim.SetInteract();

    public Vector3 GetForward() => playerModel != null ? playerModel.forward : transform.forward;

    #endregion

    #region ISkillUser

    public Vector3 Position => transform.position;
    public Vector3 Forward => GetForward();

    /// <summary>Script-driven movement without gravity (e.g. the tackle dash).</summary>
    public void Dash(Vector3 delta) => motor.Displace(delta);

    public void FaceDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (playerModel != null && direction != Vector3.zero)
            playerModel.rotation = Quaternion.LookRotation(direction);
    }

    #endregion

    #region IInteractorBody

    public void SetCanMove(bool flag)
    {
        canMove = flag;
        if (canMove)
            return;

        if (stamina != null)
            stamina.SetMoving(false);

        anim.SetMove(0);
    }

    public void Teleport(Vector3 position, Quaternion? facing = null)
    {
        motor.Teleport(position);

        if (facing.HasValue && playerModel != null)
            playerModel.rotation = facing.Value;
    }

    public void SetPhysicsEnabled(bool enabled) => motor.SetEnabled(enabled);

    #endregion

    #region IHidingAgent

    public void EnterHiding(Hidable spot, Vector3 position)
    {
        CurrentHidingPlace = spot;
        motor.Teleport(position);
        motor.SetEnabled(false);
        SetModelVisible(false);
    }

    public void ExitHiding(Vector3 position)
    {
        CurrentHidingPlace = null;
        motor.SetEnabled(false);
        motor.Teleport(position);
        motor.SetEnabled(true);
        SetModelVisible(true);
    }

    /// <summary>
    /// Only toggles the renderers: deactivating the model would also disable everything parented to it,
    /// including the interaction trigger that listens for the key to leave the hiding spot.
    /// </summary>
    private void SetModelVisible(bool isVisible)
    {
        if (playerModel == null)
            return;

        if (isVisible)
        {
            // Restore only what we hid, so renderers that were already off (debug meshes) stay off.
            foreach (Renderer hidden in hiddenRenderers)
            {
                if (hidden != null)
                    hidden.enabled = true;
            }

            hiddenRenderers.Clear();
            return;
        }

        foreach (Renderer modelRenderer in playerModel.GetComponentsInChildren<Renderer>())
        {
            if (!modelRenderer.enabled)
                continue;

            modelRenderer.enabled = false;
            hiddenRenderers.Add(modelRenderer);
        }
    }

    #endregion

    #region Collisions

    // Only fires while this CharacterController moves into something, i.e. the player walked into it.
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        IBumpable bumpable = hit.collider.GetComponentInParent<IBumpable>();
        if (bumpable != null)
            bumpable.OnBumped(transform);
    }

    #endregion

    #region IItemHolder

    public bool HasItem(ItemData item) => Inventory.HasItem(item);
    public bool AddItem(ItemData item, int amount = 1) => Inventory.AddItem(item, amount);

    #endregion
}