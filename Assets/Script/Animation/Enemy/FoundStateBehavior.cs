using UnityEngine;

public class FoundStateBehavior : StateMachineBehaviour
{
    private EnemyController ctrl;

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        EnemyController controller = GetCtrl(animator);
        if (controller != null)
            controller.NotifyFoundAnimationFinished();
    }

    private EnemyController GetCtrl(Animator anim)
    {
        if (ctrl == null)
            ctrl = anim.GetComponentInParent<EnemyController>();

        return ctrl;
    }
}
