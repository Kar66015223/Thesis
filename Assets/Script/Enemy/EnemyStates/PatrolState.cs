using System.Collections.Generic;
using UnityEngine;

public class PatrolState : EnemyStateBase
{
    private const float ArrivalTolerance = 0.1f;
    private const float PassThroughTolerance = 0.5f;

    private int curTargetIndex;
    private float nextMoveTime;
    private bool destinationSet;
    private Transform waitPoint;

    public PatrolState(EnemyController ctrl) : base(ctrl) { }

    public override void Enter()
    {
        ctrl.Movement.SetBaseSpeed(ctrl.WalkSpeed);
        ctrl.Agent.stoppingDistance = 0f;
        ctrl.Agent.updateRotation = true;
        destinationSet = false;
    }

    public override void Update()
    {
        IReadOnlyList<Waypoint> points = ctrl.PatrolPoints;
        if (points == null || points.Count == 0)
            return;

        if (Time.time < nextMoveTime)
        {
            if (waitPoint != null)
                RotateTowards(waitPoint.rotation);

            return;
        }

        curTargetIndex %= points.Count;
        Waypoint target = points[curTargetIndex];

        if (!destinationSet)
        {
            ctrl.Agent.updateRotation = true;
            waitPoint = null;

            if (target != null && target.wayPoint != null)
                ctrl.SetDestination(target.wayPoint.position);

            destinationSet = true;
            return;
        }

        bool waitHere = target == null || target.waitAtThisPoint;
        if (HasReachedDestination(waitHere ? ArrivalTolerance : PassThroughTolerance))
        {
            if (waitHere)
            {
                nextMoveTime = Time.time + ctrl.PatrolWaitTime;

                ctrl.Agent.updateRotation = false;
                waitPoint = target != null ? target.wayPoint : null;
            }

            curTargetIndex = (curTargetIndex + 1) % points.Count;
            destinationSet = false;
        }
    }

    public override void Exit()
    {
        ctrl.Agent.stoppingDistance = ApproachStoppingDistance;
        ctrl.Agent.updateRotation = true;
    }

    public override void OnHearSound(SoundSignal sound) => ctrl.ChangeState(new DistractedState(ctrl, sound));
    public override void OnSeenTarget(Transform target) => ctrl.ChangeState(new ConfirmingState(ctrl, target));
}