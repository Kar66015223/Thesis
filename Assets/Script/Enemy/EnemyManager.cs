using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public List<EnemyMovementPointPair> allEnemiesMovementPair = new();
    private Dictionary<EnemyController, EnemyMovementPointPair> dictEnemies = new();
    private Dictionary<EnemyController, int> currentStepIndex = new();

    private float toNextStepTimer;

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
        GameEvent.OnChangeAllEnemyPath += HandlePathChangeAllEnemy;
    }

    void OnDisable()
    {
        GameEvent.OnChangeEnemyPath -= HandlePathChange;
        GameEvent.OnChangeAllEnemyPath -= HandlePathChangeAllEnemy;
    }

    private void HandlePathChange(EnemyController ctrl)
    {
        if (!dictEnemies.ContainsKey(ctrl))
            return;

        currentStepIndex[ctrl]++;
        ApplyCurrentStep(ctrl);
    }
    
    private void HandlePathChangeAllEnemy()
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

                ctrl.Movement.SetSpeedModifier("Step", step.speedMultiplier);
                ctrl.tracking.StartStep(pair, index);
                break;

            case EnemyMovementType.Patrol:

                if (step.patrolPath.wayPoints == null ||
                    step.patrolPath.wayPoints.Length == 0)
                {
                    Debug.LogWarning($"{ctrl.gameObject.name} has a Patrol step with no waypoints");
                    return;
                }

                ctrl.Movement.SetSpeedModifier("Step", step.speedMultiplier);
                ctrl.tracking.StartStep(pair, index);
                break;
        }
    }
}