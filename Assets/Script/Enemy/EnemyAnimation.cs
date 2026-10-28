using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [SerializeField] private Animator anim;

    private static readonly int MoveHash = Animator.StringToHash("Move");
    private static readonly int FoundHash = Animator.StringToHash("Found");
    private static readonly int StartCatchHash = Animator.StringToHash("StartCatch");
    private static readonly int CatchingHash = Animator.StringToHash("Catching");
    // StartCatch only leaves through Catching, so a catch called off before then needs sending to the release directly.
    private static readonly int OutOfCatchStateHash = Animator.StringToHash("DemonType01_OutofCatch");
    private const float CancelCatchFadeTime = 0.25f;

    void Awake() => anim = GetComponentInChildren<Animator>();

    public void SetMove(int value) => anim.SetInteger(MoveHash, value);
    public void SetFound() => anim.SetTrigger(FoundHash);
    public void SetStartCatch() => anim.SetTrigger(StartCatchHash);
    public void SetCatching(bool value) => anim.SetBool(CatchingHash, value);

    /// <summary>The catch ended before the strangle loop started: drop the grab and play the release.</summary>
    public void CancelCatch()
    {
        anim.ResetTrigger(StartCatchHash);
        anim.SetBool(CatchingHash, false);
        anim.CrossFadeInFixedTime(OutOfCatchStateHash, CancelCatchFadeTime);
    }
}