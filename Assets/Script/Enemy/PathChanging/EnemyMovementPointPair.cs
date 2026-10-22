using System.Collections.Generic;
using UnityEngine;

public enum EnemyMovementType
{
    Idle,
    Patrol
}

[System.Serializable]
public struct PatrolPath
{
    public Waypoint[] points;
}

[System.Serializable]
public class Waypoint
{
    public Transform wayPoint;
    [Tooltip("Wait the enemy's Patrol Wait Time here before moving on. Untick to walk straight through.")]
    public bool waitAtThisPoint = true;
}

[System.Serializable]
public class EnemyMovementStep
{
    public EnemyMovementType movementType;
    
    [Tooltip("Used when Movement Type is Idle")]
    public Transform idlePoint;

    [Tooltip("Used when Movement Type is Patrol")]
    public PatrolPath patrolPath;

    public float speedMultiplier = 1f;

    [Tooltip("Idle steps only. Seconds to wait at the idle point (unable to be distracted) before moving on to the next step by itself. " +
             "0 = stay at the idle point until a level event advances the route (EnemyEvents.RequestAdvanceAllRoutes).")]
    public float toNextStepWaitTime = 0f;
}

[System.Serializable]
public class EnemyMovementPointPair
{
    public EnemyController ctrl;
    public List<EnemyMovementStep> movementSequence = new();
}