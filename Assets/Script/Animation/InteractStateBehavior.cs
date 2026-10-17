using UnityEngine;

public class InteractStateBehavior : StateMachineBehaviour
{
    private IInteractorBody body;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        => GetBody(animator)?.SetCanMove(false);

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        => GetBody(animator)?.SetCanMove(true);

    private IInteractorBody GetBody(Animator anim)
    {
        if (body == null)
            body = anim.GetComponentInParent<IInteractorBody>();

        return body;
    }
}
