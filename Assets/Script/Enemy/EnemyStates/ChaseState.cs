using Unity.VisualScripting;
using UnityEngine;

public class ChaseState : IEnemyState
{
    private EnemyController ctrl;
    private Transform target;

    public ChaseState(EnemyController ctrl, Transform target)
    {
        this.ctrl = ctrl;
        this.target = target;
    }

    public void Enter()
    {
        ctrl.Agent.speed = ctrl.runSpeed;
        ctrl.Agent.updateRotation = false;
        ctrl.Agent.isStopped = false;
    }

    public void Update()
    {
        if (target == null)
            return;

        ctrl.Agent.SetDestination(target.position);

        Vector3 direction = target.position - ctrl.transform.position;
        direction.y = 0;
        if (direction != Vector3.zero)
            ctrl.transform.rotation = Quaternion.LookRotation(direction);

        if (!ctrl.Agent.pathPending && ctrl.Agent.remainingDistance <= ctrl.Agent.stoppingDistance)
        {
            ctrl.Agent.isStopped = true;
            if (target.TryGetComponent(out PlayerController player))
            {
                ctrl.ChangeState(new CatchingState(ctrl, player));
            }
        }
    }

    public void Exit()
    {
        ctrl.Agent.updateRotation = true;
    }
    
    public void OnHearSound(SoundSignal sound) { }
    public void OnSeenTarget(Transform target) { }
}