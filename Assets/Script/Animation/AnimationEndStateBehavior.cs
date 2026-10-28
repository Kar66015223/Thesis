using UnityEngine;

/// <summary>
/// Base for state behaviours that report when their state's clip has played to the end, once per visit.
/// Uses normalizedTime instead of OnStateExit, because StartCatch and GotCaught hold their last frame until code
/// sets the parameter that leaves them. Not reported if the state is left before the end (a cancelled catch).
/// </summary>
public abstract class AnimationEndStateBehavior : StateMachineBehaviour
{
    private bool hasReported;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        => hasReported = false;

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (hasReported || stateInfo.normalizedTime < 1f)
            return;

        hasReported = true;
        OnAnimationEnd(animator);
    }

    protected abstract void OnAnimationEnd(Animator animator);
}
