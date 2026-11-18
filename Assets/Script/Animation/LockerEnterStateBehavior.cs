using UnityEngine;

public class LockerEnterStateBehavior : StateMachineBehaviour
{
    private Locker locker;

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        locker = animator.GetComponent<Locker>();
        if (locker != null)
            locker.SetHide(true);
    }
}