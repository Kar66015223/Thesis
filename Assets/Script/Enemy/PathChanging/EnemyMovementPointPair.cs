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
    public Transform[] wayPoints;
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
    public float toNextStepWaitTime = 0f;
}

[System.Serializable]
public class EnemyMovementPointPair
{
    public EnemyController ctrl;
    public List<EnemyMovementStep> movementSequence = new();
}