using UnityEngine;

/// <summary>
/// Plays the Found reaction standing still, then runs after the target until close enough to catch it,
/// or gives up when it can't reach it.
/// </summary>
public class ChaseState : EnemyStateBase
{
    private const float RepathDistance = 0.5f;
    private const float CatchRangePadding = 0.5f;
    // Safety net in case the Found animation never plays or never ends (missing transition, no Animator).
    private const float MaxFoundReactionTime = 3f;

    private readonly Transform target;
    private PlayerStruggle targetStruggle;
    private float giveUpTimer;
    private Vector3 lastTargetPos;

    private bool isReacting;
    private float reactionTimer;

    public ChaseState(EnemyController ctrl, Transform target) : base(ctrl) => this.target = target;

    public override EnemyState Indicator => EnemyState.Chase;

    public override void Enter()
    {
        ctrl.Tracking.InterruptCurrentStep();

        ctrl.Movement.SetBaseSpeed(ctrl.RunSpeed);
        ctrl.Agent.updateRotation = false;
        ctrl.Agent.stoppingDistance = ApproachStoppingDistance;
        giveUpTimer = 0f;

        if (target != null)
            targetStruggle = target.GetComponentInParent<PlayerStruggle>();

        // The chase only starts once the Found animation reports it's done (FoundStateBehavior).
        // Stopping here, not in the animation, covers the frame before the Animator actually enters Found.
        isReacting = true;
        reactionTimer = 0f;
        ctrl.Agent.isStopped = true;
        ctrl.FoundAnimationFinished += EndReaction;
        ctrl.Anim.SetFound();
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

        if (isReacting)
        {
            FaceInstantly(target.position);

            reactionTimer += Time.deltaTime;
            if (reactionTimer < MaxFoundReactionTime)
                return;

            EndReaction();
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
            // Not catchable (e.g. mid-tackle): hold position instead of grabbing.
            if (targetStruggle != null && targetStruggle.CanBeCaught)
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
        ctrl.FoundAnimationFinished -= EndReaction;

        // Left mid-reaction (stunned, gave up): undo our stop. After a catch the agent stays stopped as before.
        if (isReacting)
            ctrl.Agent.isStopped = false;

        ctrl.Agent.updateRotation = true;
        ctrl.Tracking.ResumeInterruptedStep();
    }

    private void EndReaction()
    {
        if (!isReacting)
            return;

        isReacting = false;
        ctrl.FoundAnimationFinished -= EndReaction;

        // Only now: a destination behind a closed door makes the door navigation start walking straight away.
        ctrl.Agent.isStopped = false;
        if (target != null)
        {
            lastTargetPos = target.position;
            ctrl.SetDestination(lastTargetPos);
        }
    }
}