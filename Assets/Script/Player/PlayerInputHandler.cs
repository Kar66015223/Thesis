using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput input;

    // Actions
    public event Action<Vector2> OnMoveInput;
    public event Action<Vector2> OnLookInput;
    public event Action<bool> OnRunInput;
    public event Action<bool> OnCrouchInput;
    private bool hasPressedCrouch;

    public event Action OnInteractFInput;
    public event Action OnInteractSpacebarInput;

    public event Action OnStruggleInput;

    // Skill
    public event Action OnUsePhysicalSkillInput;
    public event Action<bool> OnDemonEyeSkillInput;

    private bool isDemonEyeActivated;

    void Awake()
    {
        input = GetComponent<PlayerInput>();
        input.uiInputModule = FindAnyObjectByType<EventSystem>()
            .GetComponent<InputSystemUIInputModule>();
    }

    void OnEnable() => GameEvent.OnSwitchActionMap += SwitchActionMap;
    void OnDisable() => GameEvent.OnSwitchActionMap -= SwitchActionMap;

    public void SwitchActionMap(string mapName) => input.SwitchCurrentActionMap(mapName);

    public void OnMove(InputAction.CallbackContext context)
    {
        OnMoveInput?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        OnLookInput?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnRunInput?.Invoke(true);
        if (context.canceled)
            OnRunInput?.Invoke(false);
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            hasPressedCrouch = !hasPressedCrouch;
            OnCrouchInput?.Invoke(hasPressedCrouch);
        }
    }

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

    public void OnUseDemonEyeSkill(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isDemonEyeActivated = !isDemonEyeActivated;
            OnDemonEyeSkillInput?.Invoke(isDemonEyeActivated);
        }
    }
    
    public void OnStruggle(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnStruggleInput?.Invoke();
    }
}
