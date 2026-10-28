using UnityEngine;

/// <summary>
/// On the enemy's Found state: tells the controller when the animation ends. Stopping and resuming movement is
/// ChaseState's job, because the states set Agent.isStopped themselves and would undo it from here.
/// </summary>
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
