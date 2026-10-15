using UnityEngine;

/// <summary>Locks movement while the "Interact" animation state plays.</summary>
public class InteractStateBehavior : StateMachineBehaviour
{
    private IInteractorBody body;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        => GetBody(animator)?.SetCanMove(false);

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        => GetBody(animator)?.SetCanMove(true);

    private IInteractorBody GetBody(Animator animator)
    {
        if (body == null)
            body = animator.GetComponentInParent<IInteractorBody>();

        return body;
    }
}
