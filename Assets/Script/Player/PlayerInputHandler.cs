using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Adapter from the Input System to plain C# events.
/// It only reports what was pressed; toggles (crouch, demon eye) are owned by the systems
/// that can refuse them, so the toggle state can never get out of sync.
/// The public On... methods are wired to PlayerInput's Unity Events in the Player prefab - keep their names.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput input;

    // Actions
    public event Action<Vector2> OnMoveInput;
    public event Action<Vector2> OnLookInput;
    public event Action<bool> OnRunInput;
    public event Action OnCrouchPressed;
    public event Action<bool> OnSlowCrouchInput;

    public event Action OnInteractFInput;
    public event Action OnInteractSpacebarInput;
    /// <summary>Mouse wheel; positive = up.</summary>
    public event Action<float> OnScrollInput;

    public event Action OnStruggleInput;

    /// <summary>Bound in both the Player and UI maps so the same key closes the inventory again.</summary>
    public event Action OnInventoryPressed;

    public event Action<bool> OnPhysSkillMenuInput;
    public event Action<bool> OnPhysicalSkillInput;
    public event Action OnDemonEyeSkillPressed;

    void Awake()
    {
        input = GetComponent<PlayerInput>();

        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem != null && eventSystem.TryGetComponent(out InputSystemUIInputModule uiModule))
            input.uiInputModule = uiModule;
    }

    void OnEnable()
    {
        InputModeService.ActionMapChanged += SwitchActionMap;
        InputModeService.Refresh();
    }

    // Ask again once the scene has finished waking up: on the first frame OnEnable can run before
    // MapManager.Awake (no base/mission choice yet) or before PlayerInput turns on its Default Map.
    void Start() => InputModeService.Refresh();

    void OnDisable() => InputModeService.ActionMapChanged -= SwitchActionMap;
   
    void Update()
    {
        // Read through the Input System (not legacy Input) so all input lives in one place.
        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;

        float scroll = mouse.scroll.ReadValue().y;
        if (scroll != 0f)
            OnScrollInput?.Invoke(scroll);
    }

    private void SwitchActionMap(string mapName)
    {
        if (input.currentActionMap == null || input.currentActionMap.name != mapName)
            input.SwitchCurrentActionMap(mapName);
    }

    public void OnMove(InputAction.CallbackContext context)
        => OnMoveInput?.Invoke(context.ReadValue<Vector2>());

    public void OnLook(InputAction.CallbackContext context)
        => OnLookInput?.Invoke(context.ReadValue<Vector2>());

    public void OnRun(InputAction.CallbackContext context) => RaiseHold(context, OnRunInput);

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnCrouchPressed?.Invoke();
    }

    public void OnSlowCrouch(InputAction.CallbackContext context) => RaiseHold(context, OnSlowCrouchInput);

    public void OnInteractF(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnInteractFInput?.Invoke();
    }

    public void OnInteractSpacebar(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnInteractSpacebarInput?.Invoke();
    }

    public void OnPhysSkillMenu(InputAction.CallbackContext context) => RaiseHold(context, OnPhysSkillMenuInput);

    public void OnUsePhysicalSkill(InputAction.CallbackContext context) => RaiseHold(context, OnPhysicalSkillInput);

    public void OnUseDemonEyeSkill(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnDemonEyeSkillPressed?.Invoke();
    }

    public void OnStruggle(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnStruggleInput?.Invoke();
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnInventoryPressed?.Invoke();
    }
    
    private static void RaiseHold(InputAction.CallbackContext context, Action<bool> handler)
    {
        if (context.performed)
            handler?.Invoke(true);
        else if (context.canceled)
            handler?.Invoke(false);
    }
}
