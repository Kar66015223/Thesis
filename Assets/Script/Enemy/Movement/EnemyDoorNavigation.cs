using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyDoorNavigation : MonoBehaviour
{
    [Tooltip("Doors further than this (straight line) from the destination are left out of the route search. " +
             "Only bounds its cost; keep it large enough to include every door a route might need.")]
    [SerializeField] private float destSearchRadius = 30f;
    [SerializeField] private float doorArrivalTolerance = 0.2f;
    [SerializeField] private float repathDelay = 0.2f;
    [SerializeField] private float retryDelay = 1f;
    [SerializeField] private float destReachTolerance = 1.5f;

    private const float DoorApproachStoppingDistance = 0.1f;
    private const float MinRepathDelay = 0.05f;
    private const float DestSampleRadius = 2f;

    private NavMeshAgent agent;

    private Vector3 finalDest;
    private bool hasDest;

    private Door targetDoor;
    private Transform approachPoint;

    private bool goingToDoor;
    private bool waitingToRepath;

    private float previousStoppingDistance;
    private float repathTimer;
    private float nextDoorSearchTime;

    public bool IsHandlingDoor => goingToDoor || waitingToRepath;
    public bool IsBlocked { get; private set; }
    public bool HasArrived =>
        agent != null && hasDest && !IsHandlingDoor &&
        !agent.pathPending &&
        PathReachesDestination() &&
        agent.remainingDistance <= agent.stoppingDistance;

    public void Initialize(NavMeshAgent agent) => this.agent = agent;

    public void SetDestination(Vector3 dest)
    {
        finalDest = dest;
        hasDest = true;

        if (agent == null || goingToDoor || waitingToRepath)
            return;

        agent.SetDestination(dest);
    }

    public bool TickBeforeStateUpdate()
    {
        if (agent == null)
            return false;

        if (goingToDoor)
        {
            UpdateDoorApproach();
            return true;
        }

        if (waitingToRepath)
        {
            repathTimer -= Time.deltaTime;

            if (repathTimer <= 0f)
            {
                waitingToRepath = false;

                agent.ResetPath();
                agent.isStopped = false;

                if (hasDest)
                    agent.SetDestination(finalDest);
            }

            return true;
        }

        if (!hasDest || agent.pathPending)
            return false;

        if (PathReachesDestination())
        {
            IsBlocked = false;
            return false;
        }

        if (Time.time < nextDoorSearchTime)
            return false;

        if (TryFindDoor(out Door door, out Transform side))
        {
            IsBlocked = false;
            targetDoor = door;
            approachPoint = side;

            previousStoppingDistance = agent.stoppingDistance;
            agent.stoppingDistance = DoorApproachStoppingDistance;
            agent.isStopped = false;

            goingToDoor = true;
            agent.SetDestination(approachPoint.position);
            return true;
        }

        IsBlocked = true;
        nextDoorSearchTime = Time.time + retryDelay;

        agent.SetDestination(finalDest);
        return false;
    }

    private bool TryFindDoor(out Door door, out Transform approachPoint)
    {
        Vector3 dest = NavMesh.SamplePosition(finalDest, out NavMeshHit hit, DestSampleRadius, agent.areaMask)
            ? hit.position : finalDest;

        return DoorRoutePlanner.TryFindFirstDoor(transform.position, dest, agent.areaMask, destSearchRadius,
            out door, out approachPoint);
    }

    private void UpdateDoorApproach()
    {
        if (targetDoor == null || approachPoint == null)
        {
            AbortDoorAttempt();
            return;
        }

        if (agent.pathPending)
            return;

        if (!agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            AbortDoorAttempt();
            return;
        }

        if (agent.remainingDistance > agent.stoppingDistance + doorArrivalTolerance)
            return;

        agent.isStopped = true;

        bool opened = targetDoor.OpenForEnemy();
        if (!opened)
        {
            AbortDoorAttempt();
            return;
        }

        targetDoor = null;
        approachPoint = null;
        goingToDoor = false;

        agent.stoppingDistance = previousStoppingDistance;

        waitingToRepath = true;
        repathTimer = Mathf.Max(MinRepathDelay, repathDelay);
    }

    private void AbortDoorAttempt()
    {
        IsBlocked = true;
        goingToDoor = false;

        targetDoor = null;
        approachPoint = null;

        agent.stoppingDistance = previousStoppingDistance;
        agent.isStopped = false;

        nextDoorSearchTime = Time.time + retryDelay;

        agent.ResetPath();

        if (hasDest)
            agent.SetDestination(finalDest);
    }

    public void CancelDoorApproach()
    {
        IsBlocked = false;
        nextDoorSearchTime = 0f;

        if (!goingToDoor && !waitingToRepath)
            return;

        goingToDoor = false;
        waitingToRepath = false;

        targetDoor = null;
        approachPoint = null;

        if (agent == null)
            return;

        agent.stoppingDistance = previousStoppingDistance;
        agent.isStopped = false;
        agent.ResetPath();
    }
    
    private bool PathReachesDestination()
    {
        if (agent.pathStatus != NavMeshPathStatus.PathComplete)
            return false;

        Vector3 offset = agent.pathEndPosition - finalDest;
        offset.y = 0f;
        return offset.sqrMagnitude <= destReachTolerance * destReachTolerance;
    }
}