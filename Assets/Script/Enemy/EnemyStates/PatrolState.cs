using System.Collections.Generic;
using UnityEngine;

public class PatrolState : EnemyStateBase
{
    private const float ArrivalTolerance = 0.1f;

    private int curTargetIndex;
    private float nextMoveTime;
    private bool destinationSet;

    public PatrolState(EnemyController ctrl) : base(ctrl) { }

    public override void Enter()
    {
        ctrl.Movement.SetBaseSpeed(ctrl.WalkSpeed);
        ctrl.Agent.stoppingDistance = 0f;
        destinationSet = false;
    }

    public override void Update()
    {
        IReadOnlyList<Transform> points = ctrl.PatrolPoints;
        if (points == null || points.Count == 0 || Time.time < nextMoveTime)
            return;

        curTargetIndex %= points.Count;

        if (!destinationSet)
        {
            if (points[curTargetIndex] != null)
                ctrl.SetDestination(points[curTargetIndex].position);

            destinationSet = true;
            return;
        }

        if (HasReachedDestination(ArrivalTolerance))
        {
            nextMoveTime = Time.time + ctrl.PatrolWaitTime;
            curTargetIndex = (curTargetIndex + 1) % points.Count;
            destinationSet = false;
        }
    }

    public override void Exit() => ctrl.Agent.stoppingDistance = ApproachStoppingDistance;

    public override void OnHearSound(SoundSignal sound) => ctrl.ChangeState(new DistractedState(ctrl, sound));
    public override void OnSeenTarget(Transform target) => ctrl.ChangeState(new ConfirmingState(ctrl, target));
}