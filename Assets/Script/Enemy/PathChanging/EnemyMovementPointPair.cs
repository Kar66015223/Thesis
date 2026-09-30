using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PatrolPath
{
    public Transform[] wayPoints;
}

[System.Serializable]
public class EnemyMovementPointPair
{
    public EnemyController ctrl;
    public List<Transform> idlePoints = new();
    public List<PatrolPath> patrolPoints = new();
}