using UnityEngine;

/// <summary>Thin wrapper so gameplay code never touches Animator parameter strings.</summary>
[System.Serializable]
public class PlayerAnimation
{
    private static readonly int MoveHash = Animator.StringToHash("Move");
    private static readonly int CrouchHash = Animator.StringToHash("Crouch");
    private static readonly int InteractHash = Animator.StringToHash("Interact");

    [SerializeField] private Animator anim;

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
}
