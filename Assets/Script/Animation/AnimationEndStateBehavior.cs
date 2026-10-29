using UnityEngine;

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
