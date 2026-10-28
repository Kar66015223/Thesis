using UnityEngine;

/// <summary>On the enemy's StartCatch state: tells the controller the grab has reached the player (CatchingState waits for it).</summary>
public class StartCatchStateBehavior : AnimationEndStateBehavior
{
    private EnemyController ctrl;

    protected override void OnAnimationEnd(Animator animator)
    {
        if (ctrl == null)
            ctrl = animator.GetComponentInParent<EnemyController>();

        if (ctrl != null)
            ctrl.NotifyStartCatchAnimationFinished();
    }
}
