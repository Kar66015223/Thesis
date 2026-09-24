using UnityEngine;

public class PatrolState : IEnemyState
{
    private EnemyController ctrl;
    private int curTargetIndex;
    private float nextMoveTime;

    public PatrolState(EnemyController ctrl) 
        => this.ctrl = ctrl;

    public void Enter()
    {
        ctrl.Agent.speed = ctrl.walkSpeed;
    }

    public void Update()
    {
        if (ctrl.patrolWaypoints == null || ctrl.patrolWaypoints.Length < 1)
            return;
        if (Time.time < nextMoveTime)
            return;

        Transform target = ctrl.patrolWaypoints[curTargetIndex];
        ctrl.Agent.SetDestination(target.position);

        if (!ctrl.Agent.pathPending && ctrl.Agent.remainingDistance <= ctrl.Agent.stoppingDistance)
        {
            nextMoveTime = Time.time + ctrl.patrolWaitTime;
            curTargetIndex = (curTargetIndex + 1) % ctrl.patrolWaypoints.Length;
        }
    }

    public void Exit() { }

    public void OnHearSound(SoundSignal sound)
        => ctrl.ChangeState(new DistractedState(ctrl, sound));

    public void OnSeenTarget(Transform target)
        => ctrl.ChangeState(new ChaseState(ctrl, target));
}