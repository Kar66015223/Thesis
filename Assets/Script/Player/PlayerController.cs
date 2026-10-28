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
    [Tooltip("Crouch speed while holding the slow crouch key. The crouch walk animation slows by the same ratio.")]
    [SerializeField] private float slowCrouchSpeed = 1f;
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
    private bool isSlowCrouchHeld;
    private bool canMove = true;
    private bool isUsingSkill;

    public bool IsCrouching { get; private set; }
    /// <summary>Crouching while the slow crouch key is held, whichever of the two happened first.</summary>
    public bool IsSlowCrouching { get; private set; }
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
        inputHandler.OnSlowCrouchInput += HandleSlowCrouch;

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
        inputHandler.OnSlowCrouchInput -= HandleSlowCrouch;

        if (health != null)
        {
            health.Died -= HandleDied;
            health.Revived -= HandleRevived;
        }

        // Releases arrive while we're unsubscribed, so don't keep walking on stale input.
        moveInput = Vector2.zero;
        isRunHeld = false;
        isSlowCrouchHeld = false;
        RefreshSlowCrouch();
    }

    void Start() => anim.Initialize();

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
        RefreshSlowCrouch();
    }

    // Always store the key, even while standing: dropping presses/releases outside the crouch could leave
    // slow crouch stuck on (stand up while holding it, then let go) or never start (hold it, then crouch).
    private void HandleSlowCrouch(bool isHeld)
    {
        isSlowCrouchHeld = isHeld;
        RefreshSlowCrouch();
    }

    private void RefreshSlowCrouch()
    {
        bool isSlow = IsCrouching && isSlowCrouchHeld;
        if (isSlow == IsSlowCrouching)
            return;

        IsSlowCrouching = isSlow;
        ApplyCrouchWalkSpeed();

        PlayerEvents.RaiseSlowCrouch(isSlow);
    }

    // Same ratio as the movement, so the feet keep matching the ground.
    private void ApplyCrouchWalkSpeed()
    {
        float animSpeed = IsSlowCrouching && crouchSpeed > 0f ? slowCrouchSpeed / crouchSpeed : 1f;
        anim.SetCrouchWalkSpeed(animSpeed);
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
            return IsSlowCrouching ? slowCrouchSpeed : crouchSpeed;

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

    #region Caught

    public Transform GetBone(HumanBodyBones bone) => anim.GetBone(bone);

    /// <summary>
    /// An enemy has the player by the neck: stop moving and colliding (PlayerStruggle keeps the body in the enemy's
    /// hand, and a CharacterController would fight that), face the enemy and play GotCaught.
    /// Returns false if there's no GotCaught animation to wait for.
    /// </summary>
    public bool BeginCaught(Vector3 catcherPosition)
    {
        SetCanMove(false);
        motor.SetEnabled(false);
        FaceDirection(catcherPosition - transform.position);

        return anim.PlayGotCaught();
    }

    public void PlayCaughtLoop() => anim.PlayGotCaughtLoop();

    public void EndCaught()
    {
        anim.StopGotCaught();

        // Swapping the controller back reset its parameters: restore the stance the player is still in.
        anim.SetCrouch(IsCrouching);
        ApplyCrouchWalkSpeed();

        motor.SetEnabled(true);

        // A player who died in the enemy's hands stays frozen for the game over screen.
        if (health == null || !health.IsDead)
            SetCanMove(true);
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