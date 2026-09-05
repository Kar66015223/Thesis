using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    // Actions
    public event Action<Vector2> OnMoveInput;
    public event Action<Vector2> OnLookInput;
    public event Action<bool> OnRunInput;

    public event Action OnInteractInput;

    // Skill
    public event Action OnUsePhysicalSkillInput;
    public event Action OnUseDemonEyeSkillInput;

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

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnInteractInput?.Invoke();
    }
    
    public void OnUseDemonEyeSkill(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnUseDemonEyeSkillInput?.Invoke();
    }
}
