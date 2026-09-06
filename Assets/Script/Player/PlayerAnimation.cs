using UnityEngine;

[System.Serializable]
public class PlayerAnimation
{
    [SerializeField] private Animator anim;

    private readonly int MoveHash = Animator.StringToHash("Move");
    private readonly int InteractHash = Animator.StringToHash("Interact");

    public void SetMove(int value)
    {
        anim.SetInteger(MoveHash, value);
    }

    public void SetInteract()
    {
        anim.SetTrigger(InteractHash);
    }
}
