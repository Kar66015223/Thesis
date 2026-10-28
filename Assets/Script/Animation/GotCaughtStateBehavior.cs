using UnityEngine;

/// <summary>On the player's GotCaught state (Caught controller): tells PlayerStruggle the grab animation is over.</summary>
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
