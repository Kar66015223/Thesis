using UnityEditor.Animations;
using UnityEngine;

/// <summary>Thin wrapper so gameplay code never touches Animator parameter strings.</summary>
[System.Serializable]
public class PlayerAnimation
{
    [SerializeField] private Animator anim;
    [SerializeField] private AnimatorController baseAnimCtrl;
    [SerializeField] private AnimatorController missionAnimCtrl;

    private static readonly int MoveHash = Animator.StringToHash("Move");
    private static readonly int CrouchHash = Animator.StringToHash("Crouch");
    private static readonly int InteractHash = Animator.StringToHash("Interact");
    private static readonly int CrouchWalkSpeedHash = Animator.StringToHash("CrouchWalkSpeed");

    public void Initialize()
    {
        if (MapManager.Instance == null || MapManager.Instance.DefaultActionMapName == "")
            return;

        if (MapManager.Instance.DefaultActionMapName == PlayerConstants.ACTIONMAP_PLAYER_BASE)
            SetAnimCtrl(baseAnimCtrl);
        else
            SetAnimCtrl(missionAnimCtrl);
    }

    public void SetAnimCtrl(AnimatorController animCtrl)
    {
        anim.runtimeAnimatorController = animCtrl;
    }

    public void SetMove(int value)
    {
        if (anim != null)
            anim.SetInteger(MoveHash, value);
    }

    public void SetInteract()
    {
        if (anim != null)
            anim.SetTrigger(InteractHash);
    }

    public void SetCrouch(bool value)
    {
        if (anim != null)
            anim.SetBool(CrouchHash, value);
    }

    /// <summary>
    /// Playback speed of the crouch walk state only (its Speed Multiplier parameter),
    /// unlike Animator.speed, which would also slow every other animation.
    /// </summary>
    public void SetCrouchWalkSpeed(float value)
    {
        if (anim != null)
            anim.SetFloat(CrouchWalkSpeedHash, value);
    }
}
