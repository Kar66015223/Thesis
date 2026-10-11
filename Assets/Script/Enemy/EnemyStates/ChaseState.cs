using UnityEngine;

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

        ctrl.SetDestination(target.position);
        lastTargetPos = target.position;

        GameEvent.OnShowEnemyState?.Invoke(ctrl.transform, EnemyState.Chase);
    }

    public void Update()
    {
        if (target == null)
            return;

        if (Vector3.Distance(target.position, lastTargetPos) > 0.5f)
        {
            ctrl.SetDestination(target.position);
            lastTargetPos = target.position;
        }

        Vector3 direction = target.position - ctrl.transform.position;
        direction.y = 0;
        if (direction != Vector3.zero)
            ctrl.transform.rotation = Quaternion.LookRotation(direction);

        float actualDistance = direction.magnitude;
        float catchDistance  = ctrl.Agent.stoppingDistance + 0.5f;

        EnemyDoorNavigation doorNav = ctrl.DoorNavigation;
        bool reachedNavMeshDest = doorNav.HasArrived;
        bool isInvalidOrPartialPath = doorNav.IsBlocked;

        PlayerStruggle player = target.GetComponentInParent<PlayerStruggle>();

        if (player != null && player.IsCatched && player.CatchingEnemy != ctrl)
        {
            ctrl.ReturnToDefaultState();
            return;
        }

        if (actualDistance <= catchDistance)
        {
            ctrl.Agent.isStopped = true;
            if (player != null && !player.IsCatched)
            {
                ctrl.ChangeState(new CatchingState(ctrl, player));
            }
        }
        else if (reachedNavMeshDest || isInvalidOrPartialPath)
        {
            giveUpTimer += Time.deltaTime;
            if (giveUpTimer >= ctrl.distractWaitTime)
            {
                ctrl.ReturnToDefaultState();
            }

            Debug.Log("Path invalid");
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
        GameEvent.OnShowEnemyState?.Invoke(ctrl.transform, EnemyState.None);

        ctrl.tracking.ResumeInterruptedStep();
    }
    
    public void OnHearSound(SoundSignal sound) { }
    public void OnSeenTarget(Transform target) { }
}