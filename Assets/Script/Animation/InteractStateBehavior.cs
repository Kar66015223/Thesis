using UnityEngine;

public class InteractStateBehavior : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerController controller = animator.GetComponentInParent<PlayerController>();
        if (controller != null)
        {
            controller.SetCanMove(false);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerController controller = animator.GetComponentInParent<PlayerController>();
        if (controller != null)
        {
            controller.SetCanMove(true);
        }
    }
}
