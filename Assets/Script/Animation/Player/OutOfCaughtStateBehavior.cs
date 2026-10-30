using UnityEngine;

public class OutOfCaughtStateBehavior : AnimationEndStateBehavior
{
    private PlayerController ctrl;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        if (ctrl == null)
            ctrl = animator.GetComponentInParent<PlayerController>();

        if (ctrl != null)
            ctrl.SetCanMove(false);
    }
    
    protected override void OnAnimationEnd(Animator animator)
    {
        if (ctrl == null)
            ctrl = animator.GetComponentInParent<PlayerController>();

        if (ctrl != null)
        {
            ctrl.BackToDefaultAnimCtrl();
            ctrl.SetCanMove(true);
        }
    }
}