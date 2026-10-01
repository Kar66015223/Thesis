using UnityEngine;

public class IdleState : IEnemyState
{
    private EnemyController ctrl;
    private bool arrived = false;

    public IdleState(EnemyController ctrl) 
        => this.ctrl = ctrl;

    public void Enter()
    {
        ctrl.Movement.SetBaseSpeed(ctrl.walkSpeed);
        ctrl.Agent.isStopped = false;
        ctrl.Agent.updateRotation = true;
        ctrl.Agent.stoppingDistance = 0f;

        ctrl.Agent.SetDestination(ctrl.idlePoint.position);
        arrived = false;
    }

    public void Update()
    {
        if(ctrl.tracking.IsWaiting)
        {
            ctrl.tracking.UpdateWaiting();
            if (ctrl.idlePoint != null)
            {
                ctrl.transform.rotation = Quaternion.Slerp(
                    ctrl.transform.rotation,
                    ctrl.idlePoint.rotation,
                    ctrl.lookAtRotationSpeed * Time.deltaTime);
            }

            return;
        }

        if (!arrived && !ctrl.Agent.pathPending && ctrl.Agent.remainingDistance <= ctrl.Agent.stoppingDistance)
        {
            arrived = true;
            float wait = ctrl.tracking.GetActiveStepWaitTime();

            if (wait > 0f)
                ctrl.tracking.StartIdleWait(wait);
        }

        if(arrived && !ctrl.tracking.IsWaiting)
        {
            if(ctrl.idlePoint != null)
            {
                ctrl.transform.rotation = Quaternion.Slerp(
                    ctrl.transform.rotation,
                    ctrl.idlePoint.rotation,
                    ctrl.lookAtRotationSpeed * Time.deltaTime);
            }
        }
    }

    public void Exit() {}

    public void OnHearSound(SoundSignal sound)
    {
        if (ctrl.tracking.IsWaiting)
            return;

        ctrl.ChangeState(new DistractedState(ctrl, sound));
    }

    public void OnSeenTarget(Transform target)
    {
        if (ctrl.tracking.IsWaiting)
            return;

        ctrl.ChangeState(new ConfirmingState(ctrl, target));
    }
}