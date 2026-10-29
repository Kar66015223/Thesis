using UnityEngine;

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
