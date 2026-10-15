using UnityEngine;

/// <summary>Runs after the target until close enough to catch it, or gives up when it can't reach it.</summary>
public class ChaseState : EnemyStateBase
{
    private const float RepathDistance = 0.5f;
    private const float CatchRangePadding = 0.5f;

    private readonly Transform target;
    private PlayerStruggle targetStruggle;
    private float giveUpTimer;
    private Vector3 lastTargetPos;

    public ChaseState(EnemyController ctrl, Transform target) : base(ctrl) => this.target = target;

    public override EnemyState Indicator => EnemyState.Chase;

    public override void Enter()
    {
        ctrl.Tracking.InterruptCurrentStep();

        ctrl.Movement.SetBaseSpeed(ctrl.RunSpeed);
        ctrl.Agent.updateRotation = false;
        ctrl.Agent.isStopped = false;
        ctrl.Agent.stoppingDistance = ApproachStoppingDistance;
        giveUpTimer = 0f;

        if (target == null)
            return;

        targetStruggle = target.GetComponentInParent<PlayerStruggle>();
        lastTargetPos = target.position;
        ctrl.SetDestination(lastTargetPos);
    }

    public override void Update()
    {
        if (target == null)
            return;

        // Someone else already caught the player.
        if (targetStruggle != null && targetStruggle.IsCaught && !ReferenceEquals(targetStruggle.CatchingEnemy, ctrl))
        {
            ctrl.ReturnToDefaultState();
            return;
        }

        if (Vector3.Distance(target.position, lastTargetPos) > RepathDistance)
        {
            lastTargetPos = target.position;
            ctrl.SetDestination(lastTargetPos);
        }

        FaceInstantly(target.position);

        float distance = FlatDirectionTo(target.position).magnitude;
        float catchDistance = ctrl.Agent.stoppingDistance + CatchRangePadding;

        if (distance <= catchDistance)
        {
            ctrl.Agent.isStopped = true;
            if (targetStruggle != null && !targetStruggle.IsCaught)
                ctrl.ChangeState(new CatchingState(ctrl, targetStruggle));
            return;
        }

        ctrl.Agent.isStopped = false;

        EnemyDoorNavigation doorNav = ctrl.DoorNavigation;
        if (doorNav.HasArrived || doorNav.IsBlocked)
        {
            // Reached the last known spot (or can't get there) without catching: give up after a moment.
            giveUpTimer += Time.deltaTime;
            if (giveUpTimer >= ctrl.DistractWaitTime)
                ctrl.ReturnToDefaultState();
        }
        else if (!ctrl.Agent.pathPending)
        {
            giveUpTimer = 0f;
        }
    }

    public override void Exit()
    {
        ctrl.Agent.updateRotation = true;
        ctrl.Tracking.ResumeInterruptedStep();
    }
}
