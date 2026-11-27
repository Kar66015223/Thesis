using UnityEngine;

public class InteractStateBehavior : StateMachineBehaviour
{
    private IInteractorBody body;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        IInteractorBody target = GetBody(animator);
        if (target == null)
            return;

        target.SetCanMove(false);
        target.SetInteracting(true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        IInteractorBody target = GetBody(animator);
        if (target == null)
            return;

        target.SetCanMove(true);
        target.SetInteracting(false);
    }

    private IInteractorBody GetBody(Animator anim)
    {
        if (body == null)
            body = anim.GetComponentInParent<IInteractorBody>();

        return body;
    }
}
