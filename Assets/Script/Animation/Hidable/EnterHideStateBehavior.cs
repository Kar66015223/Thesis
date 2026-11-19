using UnityEngine;

public class EnterHideStateBehavior : StateMachineBehaviour
{
    private Hidable hidable;

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        hidable = animator.GetComponentInParent<Hidable>();
        if (hidable != null)
            hidable.SetHide(true);
    }
}