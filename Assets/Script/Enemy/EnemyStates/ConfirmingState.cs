
using Unity.VisualScripting;
using UnityEngine;

public class ConfirmingState : IEnemyState
{
    private EnemyController ctrl;
    private Transform target;
    private float stopTimer;

    public ConfirmingState(EnemyController ctrl, Transform target)
    {
        this.ctrl = ctrl;
        this.target = target;
    }

    public void Enter()
    {
        ctrl.Agent.speed = ctrl.walkSpeed;
        ctrl.Agent.isStopped = true;
        ctrl.Agent.updateRotation = false;

        GameEvent.OnAlertEnemyState?.Invoke(ctrl.transform, EnemyState.Confirming);
    }

    public void Update()
    {
        if (target == null)
            return;

        Vector3 direction = target.position - ctrl.transform.position;
        direction.y = 0;
        if (direction != Vector3.zero)
            ctrl.transform.rotation = Quaternion.LookRotation(direction);
    }

    public void Exit()
    {
        ctrl.Agent.updateRotation = true;
        ctrl.Agent.isStopped = false;
    }

    public void OnHearSound(SoundSignal sound) {}

    public void OnSeenTarget(Transform target)
        => ctrl.ChangeState(new ChaseState(ctrl, target));
}