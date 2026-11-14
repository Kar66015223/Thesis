using UnityEngine;

public class HungryStateBehavior : StateMachineBehaviour
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

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (ctrl != null)
            ctrl.SetCanMove(true);
    }
}