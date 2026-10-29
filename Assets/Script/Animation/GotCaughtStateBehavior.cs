using UnityEngine;

public class GotCaughtStateBehavior : AnimationEndStateBehavior
{
    private PlayerStruggle struggle;

    protected override void OnAnimationEnd(Animator animator)
    {
        if (struggle == null)
            struggle = animator.GetComponentInParent<PlayerStruggle>();

        if (struggle != null)
            struggle.NotifyGotCaughtAnimationFinished();
    }
}
