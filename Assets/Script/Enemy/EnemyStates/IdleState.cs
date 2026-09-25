using UnityEngine;

public class IdleState : IEnemyState
{
    private EnemyController ctrl;
    // private int curTargetIndex;
    // private float nextMoveTime;

    public IdleState(EnemyController ctrl) 
        => this.ctrl = ctrl;

    public void Enter()
    {
        ctrl.Agent.speed = ctrl.walkSpeed;
        ctrl.Agent.isStopped = false;
        ctrl.Agent.updateRotation = true;
        ctrl.Agent.stoppingDistance = 0f;
        
        ctrl.Agent.SetDestination(ctrl.idleStandPoint.position);
    }

    public void Update()
    {
        if (!ctrl.Agent.pathPending && ctrl.Agent.remainingDistance <= ctrl.Agent.stoppingDistance)
        {
            ctrl.transform.rotation = Quaternion.Slerp(
                ctrl.transform.rotation,
                ctrl.idleStandPoint.rotation,
                ctrl.lookAtRotationSpeed * Time.deltaTime);   
        }    
    }

    public void Exit() {}

    public void OnHearSound(SoundSignal sound)
        => ctrl.ChangeState(new DistractedState(ctrl, sound));

    public void OnSeenTarget(Transform target)
        => ctrl.ChangeState(new ChaseState(ctrl, target));
}