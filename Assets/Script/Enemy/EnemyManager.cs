using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public List<EnemyMovementPointPair> allEnemiesMovementPair = new();
    private Dictionary<EnemyController, EnemyMovementPointPair> dictEnemies = new();
    private Dictionary<EnemyController, int> currentStepIndex = new();

    void Awake()
    {
        foreach (var pair in allEnemiesMovementPair)
        {
            if (pair.ctrl == null)
                continue;

            dictEnemies[pair.ctrl] = pair;
            currentStepIndex[pair.ctrl] = 0;
        }
    }

    void Start()
    {
        foreach (var pair in dictEnemies.Values)
        {
            ApplyCurrentStep(pair.ctrl);
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
        foreach (var pair in dictEnemies.Values)
        {
            EnemyController ctrl = pair.ctrl;
            currentStepIndex[ctrl]++;
            ApplyCurrentStep(ctrl);
        }
    }

    private void ApplyCurrentStep(EnemyController ctrl)
    {
        EnemyMovementPointPair pair = dictEnemies[ctrl];
        int index = currentStepIndex[ctrl];

        if (index >= pair.movementSequence.Count)
        {
            Debug.Log($"{ctrl.gameObject.name} has completed its movement sequence");
            return;
        }

        EnemyMovementStep step = pair.movementSequence[index];

        switch(step.movementType)
        {
            case EnemyMovementType.Idle:

                if (step.idlePoint == null)
                {
                    Debug.LogWarning($"{ctrl.gameObject.name} has an Idle step with no idle point");
                    return;
                }

                ctrl.SetIdlePoint(step.idlePoint);
                break;

            case EnemyMovementType.Patrol:
            
                if (step.patrolPath.wayPoints == null ||
                    step.patrolPath.wayPoints.Length == 0)
                {
                    Debug.LogWarning($"{ctrl.gameObject.name} has a Patrol step with no waypoints");
                    return;
                }

                ctrl.SetPatrolPoints(step.patrolPath.wayPoints);
                break;
        }
    }
}