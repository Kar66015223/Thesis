using UnityEngine;
using UnityEngine.AI;

public class ChaseState : IEnemyState
{
    private EnemyController ctrl;
    private Transform target;
    private float giveUpTimer;
    private Vector3 lastTargetPos;

    public ChaseState(EnemyController ctrl, Transform target)
    {
        this.ctrl = ctrl;
        this.target = target;
    }

    public void Enter()
    {
        ctrl.tracking.InterruptCurrentStep();

        ctrl.Movement.SetBaseSpeed(ctrl.runSpeed);
        ctrl.Agent.updateRotation = false;
        ctrl.Agent.isStopped = false;
        ctrl.Agent.stoppingDistance = 2f;
        giveUpTimer = 0f;

        ctrl.Agent.SetDestination(target.position);
        lastTargetPos = target.position;

        GameEvent.OnAlertEnemyState?.Invoke(ctrl.transform, EnemyState.Chase);
    }

    public void Update()
    {
        if (target == null)
            return;

        if (Vector3.Distance(target.position, lastTargetPos) > 0.5f)
        {
            ctrl.Agent.SetDestination(target.position);
            lastTargetPos = target.position;
        }

        Vector3 direction = target.position - ctrl.transform.position;
        direction.y = 0;
        if (direction != Vector3.zero)
            ctrl.transform.rotation = Quaternion.LookRotation(direction);

        float actualDistance = Vector3.Distance(ctrl.transform.position, target.position);

        bool reachedNavMeshDestination =
            !ctrl.Agent.pathPending && ctrl.Agent.remainingDistance <= ctrl.Agent.stoppingDistance;

        bool isInvalidOrPartialPath =
            !ctrl.Agent.pathPending &&
            (ctrl.Agent.pathStatus == NavMeshPathStatus.PathInvalid ||
                ctrl.Agent.pathStatus == NavMeshPathStatus.PathPartial);

        if (actualDistance <= ctrl.Agent.stoppingDistance)
        {
            ctrl.Agent.isStopped = true;
            if (target.TryGetComponent(out PlayerController player))
            {
                ctrl.ChangeState(new CatchingState(ctrl, player));
            }
        }
        else if (reachedNavMeshDestination || isInvalidOrPartialPath)
        {
            giveUpTimer += Time.deltaTime;
            if (giveUpTimer >= ctrl.distractWaitTime)
            {
                ctrl.ChangeState(ctrl.initialState);
            }
        }
        else
        {
            if(!ctrl.Agent.pathPending)
                giveUpTimer = 0f;
        }
    }

    public void Exit()
    {
        ctrl.Agent.updateRotation = true;
        GameEvent.OnAlertEnemyState?.Invoke(ctrl.transform, EnemyState.None);

        ctrl.tracking.ResumeInterruptedStep();
    }
    
    public void OnHearSound(SoundSignal sound) { }
    public void OnSeenTarget(Transform target) { }
}