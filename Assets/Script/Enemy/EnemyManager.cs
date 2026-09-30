using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public List<EnemyMovementPointPair> listIdlePairs = new();
    private Dictionary<EnemyController, List<Transform>> dictIdlePairs = new();

    public List<EnemyMovementPointPair> listPatrolPairs = new();
    private Dictionary<EnemyController, List<PatrolPath>> dictPatrolPairs = new();

    private int currentPathIndex = 0;

    void Awake()
    {
        foreach (var idlePair in listIdlePairs)
        {
            if (!dictIdlePairs.ContainsKey(idlePair.ctrl))
            {
                dictIdlePairs.Add(idlePair.ctrl, idlePair.idlePoints);
            }
        }

        foreach (var patrolPair in listPatrolPairs)
        {
            if (!dictPatrolPairs.ContainsKey(patrolPair.ctrl))
            {
                dictPatrolPairs.Add(patrolPair.ctrl, patrolPair.patrolPoints);
            }
        }
    }

    void OnEnable()
    {
        GameEvent.OnChangeEnemyPath += HandlePathChange;
    }

    void OnDisable()
    {
        GameEvent.OnChangeEnemyPath -= HandlePathChange;
    }

    private void HandlePathChange()
    {
        foreach (var ctrl in dictIdlePairs.Keys)
            ChangeEnemyIdlePoint(ctrl, currentPathIndex);

        foreach (var ctrl in dictPatrolPairs.Keys)
            ChangeEnemyPatrolPoint(ctrl, currentPathIndex);

        currentPathIndex++;
    }

    public void ChangeEnemyIdlePoint(EnemyController ctrl, int index)
    {
        if(dictIdlePairs.ContainsKey(ctrl))
        {
            var idlePointList = dictIdlePairs[ctrl];

            if (index < idlePointList.Count)
            {
                Transform newIdlePoint = idlePointList[index];
                ctrl.SetIdlePoint(newIdlePoint);
            }
            else
                Debug.LogWarning($"Index {index} is out of bounds for idle points of {ctrl.gameObject.name}");
        }
    }
    
    public void ChangeEnemyPatrolPoint(EnemyController ctrl, int index)
    {
        if(dictPatrolPairs.ContainsKey(ctrl))
        {
            var patrolPointsList = dictPatrolPairs[ctrl];

            if (index < patrolPointsList.Count)
            {
                Transform[] newPatrolPoints = patrolPointsList[index].wayPoints;
                ctrl.SetPatrolPoints(newPatrolPoints);
            }
            else    
                Debug.LogWarning($"Index {index} is out of bounds for patrol points of {ctrl.gameObject.name}");
        }
    }
}