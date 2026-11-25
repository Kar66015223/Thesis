using UnityEngine;
using UnityEngine.AI;

public class SearchState : EnemyStateBase
{
    private const int MaxPickAttempt = 10;
    private const float NavMeshSnapDistance = 2f;

    private Vector3 center;
    public Vector3 Center => center;
    private readonly Vector3? startCenter;

    private bool isWaiting;
    private float waitTimer;

    private float searchTimer;

    public SearchState(EnemyController ctrl, Vector3? startCenter = null) : base(ctrl) =>
        this.startCenter = startCenter;

    public override void Enter()
    {
        ctrl.Tracking.InterruptCurrentStep();
        ctrl.Alert();

        ctrl.Movement.SetBaseSpeed(ctrl.FastWalkSpeed);
        ctrl.Agent.isStopped = false;
        ctrl.Agent.updateRotation = true;
        ctrl.Agent.stoppingDistance = ApproachStoppingDistance;

        center = startCenter ?? ctrl.Vision.LastSeenPosition ?? ctrl.transform.position;
        searchTimer = 0f;
        ctrl.SetDestination(center);
    }

    public override void Update()
    {
        searchTimer += Time.deltaTime;
        if(searchTimer >= ctrl.SearchTime)
        {
            ctrl.ReturnToDefaultState();
            return;
        }

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer < ctrl.SearchWaitTime)
                return;

            isWaiting = false;
            ctrl.SetDestination(PickSearchPoint());
            return;
        }
        
        EnemyDoorNavigation doorNav = ctrl.DoorNavigation;
        if (doorNav.HasArrived || (doorNav.IsBlocked && HasReachedDestination()))
        {
            isWaiting = true;
            waitTimer = 0f;
        }
    }

    public override void Exit() => ctrl.Tracking.ResumeInterruptedStep();
    public override void OnSeenTarget(Transform target) => ReactToSighting(target);
    public override void OnHearSound(SoundSignal sound)
    {
        center = sound.Position;
        ctrl.SetDestination(center);
        isWaiting = false;
    }
    
    private Vector3 PickSearchPoint()
    {
        Vector3 farthestPoint = center;
        float farthestDistance = -1f;

        for (int i = 0; i < MaxPickAttempt; i++)
        {
            Vector3 offset = Random.insideUnitCircle * ctrl.MaxSearchRadius;
            Vector3 candidate = center + new Vector3(offset.x, 0f, offset.y);

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, NavMeshSnapDistance, ctrl.Agent.areaMask))
                continue;

            float distance = FlatDirectionTo(hit.position).magnitude;
            if (distance >= ctrl.MinSearchDistance)
                return hit.position;

            if(distance > farthestDistance)
            {
                farthestPoint = hit.position;
                farthestDistance = distance;
            }
        }

        return farthestPoint;
    }
}