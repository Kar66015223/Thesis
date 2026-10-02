using UnityEngine;

public class PatrolState : IEnemyState
{
    private EnemyController ctrl;
    private int curTargetIndex;
    private float nextMoveTime;

    private bool destinationSet;

    public PatrolState(EnemyController ctrl) 
        => this.ctrl = ctrl;

    public void Enter()
    {
        ctrl.Movement.SetBaseSpeed(ctrl.walkSpeed);
        ctrl.Agent.stoppingDistance = 0f;
        destinationSet = false;
    }

    public void Update()
    {
        var points = ctrl.patrolPoints;
        if (points == null || points.Length == 0 || Time.time < nextMoveTime) return;

        curTargetIndex %= points.Length;

        if (!destinationSet)
        {
            ctrl.Agent.SetDestination(points[curTargetIndex].position);
            destinationSet = true;
            return;
        }

        if (!ctrl.Agent.pathPending &&
            ctrl.Agent.remainingDistance <= ctrl.Agent.stoppingDistance + 0.1f)
        {
            nextMoveTime = Time.time + ctrl.patrolWaitTime;
            curTargetIndex = (curTargetIndex + 1) % points.Length;
            destinationSet = false;
        }
    }

    public void Exit()
    {
        ctrl.Agent.stoppingDistance = 2f;
    }

    public void OnHearSound(SoundSignal sound)
        => ctrl.ChangeState(new DistractedState(ctrl, sound));

    public void OnSeenTarget(Transform target)
        => ctrl.ChangeState(new ConfirmingState(ctrl, target));
}