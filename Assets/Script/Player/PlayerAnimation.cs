using UnityEngine;

[System.Serializable]
public class PlayerAnimation
{
    [SerializeField] private Animator anim;
    [SerializeField] private RuntimeAnimatorController baseAnimCtrl;
    [SerializeField] private RuntimeAnimatorController missionAnimCtrl;
    [Tooltip("Swapped in while an enemy holds the player: starts on GotCaught, loops while 'Caught' is on.")]
    [SerializeField] private RuntimeAnimatorController caughtAnimCtrl;

    private RuntimeAnimatorController controllerBeforeCaught;

    private static readonly int MoveHash = Animator.StringToHash("Move");
    private static readonly int CrouchHash = Animator.StringToHash("Crouch");
    private static readonly int InteractHash = Animator.StringToHash("Interact");
    private static readonly int CrouchWalkSpeedHash = Animator.StringToHash("CrouchWalkSpeed");

    private static readonly int CaughtHash = Animator.StringToHash("Caught");

    public void Initialize()
    {
        if (MapManager.Instance == null || MapManager.Instance.DefaultActionMapName == "")
            return;

        if (MapManager.Instance.DefaultActionMapName == PlayerConstants.ACTIONMAP_PLAYER_BASE)
            SetAnimCtrl(baseAnimCtrl);
        else
            SetAnimCtrl(missionAnimCtrl);
    }

    public void SetAnimCtrl(RuntimeAnimatorController animCtrl)
    {
        anim.runtimeAnimatorController = animCtrl;
    }

    public Transform GetBone(HumanBodyBones bone) => anim != null && anim.isHuman ? anim.GetBoneTransform(bone) : null;

    public bool PlayGotCaught()
    {
        if (anim == null || caughtAnimCtrl == null)
            return false;

        if (anim.runtimeAnimatorController != caughtAnimCtrl)
            controllerBeforeCaught = anim.runtimeAnimatorController;

        SetAnimCtrl(caughtAnimCtrl);
        anim.SetBool(CaughtHash, false);
        return true;
    }

    public void PlayGotCaughtLoop()
    {
        if (anim != null && anim.runtimeAnimatorController == caughtAnimCtrl)
            anim.SetBool(CaughtHash, true);
    }

    public void StopGotCaught()
    {
        if (anim != null && anim.runtimeAnimatorController == caughtAnimCtrl && controllerBeforeCaught != null)
            anim.SetBool(CaughtHash, false);
        // SetAnimCtrl(controllerBeforeCaught);
    }

    public void BackToDefaultAnimCtrl() => SetAnimCtrl(controllerBeforeCaught);

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

    public void SetCrouchWalkSpeed(float value)
    {
        if (anim != null)
            anim.SetFloat(CrouchWalkSpeedHash, value);
    }
}
