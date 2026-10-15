using UnityEngine;

/// <summary>Walks to the idle point, optionally waits there, then faces the point's direction.</summary>
public class IdleState : EnemyStateBase
{
    private bool arrived;

    public IdleState(EnemyController ctrl) : base(ctrl) { }

    public override void Enter()
    {
        ctrl.Movement.SetBaseSpeed(ctrl.WalkSpeed);
        ctrl.Agent.isStopped = false;
        ctrl.Agent.updateRotation = true;
        ctrl.Agent.stoppingDistance = 0f;

        if (ctrl.IdlePoint != null)
            ctrl.SetDestination(ctrl.IdlePoint.position);

        arrived = false;
    }

    public override void Update()
    {
        EnemyStepTracking tracking = ctrl.Tracking;

        if (tracking.IsWaiting)
        {
            tracking.UpdateWaiting();
            FaceIdleDirection();
            return;
        }

        if (!arrived && HasReachedDestination())
        {
            arrived = true;

            float wait = tracking.ActiveStepWaitTime;
            if (wait > 0f)
                tracking.StartIdleWait(wait);
        }

        if (arrived && !tracking.IsWaiting)
            FaceIdleDirection();
    }

    // Scripted waits (e.g. guarding the circuit board) can't be interrupted.
    public override void OnHearSound(SoundSignal sound)
    {
        if (!ctrl.Tracking.IsWaiting)
            ctrl.ChangeState(new DistractedState(ctrl, sound));
    }

    public override void OnSeenTarget(Transform target)
    {
        if (!ctrl.Tracking.IsWaiting)
            ctrl.ChangeState(new ConfirmingState(ctrl, target));
    }

    private void FaceIdleDirection()
    {
        if (ctrl.IdlePoint != null)
            RotateTowards(ctrl.IdlePoint.rotation);
    }
}
