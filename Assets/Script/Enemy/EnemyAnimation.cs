using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [SerializeField] private Animator anim;

    private static readonly int MoveHash = Animator.StringToHash("Move");
    private static readonly int FoundHash = Animator.StringToHash("Found");
    private static readonly int StartCatchHash = Animator.StringToHash("StartCatch");
    private static readonly int CatchingHash = Animator.StringToHash("Catching");

    void Awake() => anim = GetComponentInChildren<Animator>();

    public void SetMove(int value) => anim.SetInteger(MoveHash, value);
    public void SetFound() => anim.SetTrigger(FoundHash);
    public void SetStartCatch() => anim.SetTrigger(StartCatchHash);
    public void SetCatching(bool value) => anim.SetBool(CatchingHash, value);
}