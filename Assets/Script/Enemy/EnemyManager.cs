using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Feeds each enemy its scripted route, one step at a time. An enemy advances when its own timed
/// wait finishes, or all enemies advance together when a level event asks for it.
/// </summary>
public class EnemyManager : MonoBehaviour
{
    [SerializeField] private List<EnemyMovementPointPair> allEnemiesMovementPair = new();

    private readonly Dictionary<EnemyController, EnemyMovementPointPair> routes = new();
    private readonly Dictionary<EnemyController, int> stepIndex = new();

    void Awake()
    {
        foreach (EnemyMovementPointPair pair in allEnemiesMovementPair)
        {
            if (pair.ctrl == null)
                continue;

            routes[pair.ctrl] = pair;
            stepIndex[pair.ctrl] = 0;
        }
    }

    void OnEnable()
    {
        EnemyEvents.AdvanceAllRoutesRequested += AdvanceAll;
        foreach (EnemyController enemy in routes.Keys)
            enemy.Tracking.StepWaitCompleted += Advance;
    }

    void OnDisable()
    {
        EnemyEvents.AdvanceAllRoutesRequested -= AdvanceAll;
        foreach (EnemyController enemy in routes.Keys)
        {
            if (enemy != null)
                enemy.Tracking.StepWaitCompleted -= Advance;
        }
    }

    void Start()
    {
        foreach (EnemyController enemy in routes.Keys)
            ApplyCurrentStep(enemy);
    }

    private void Advance(EnemyController enemy)
    {
        if (!routes.ContainsKey(enemy))
            return;

        stepIndex[enemy]++;
        ApplyCurrentStep(enemy);
    }

    private void AdvanceAll()
    {
        // Copy the keys: advancing can't change the dictionary, but this keeps it safe if it ever does.
        foreach (EnemyController enemy in new List<EnemyController>(routes.Keys))
            Advance(enemy);
    }

    private void ApplyCurrentStep(EnemyController enemy)
    {
        EnemyMovementPointPair route = routes[enemy];
        int index = stepIndex[enemy];

        if (index >= route.movementSequence.Count)
        {
            Debug.Log($"{enemy.name} has completed its movement sequence", enemy);
            return;
        }

        EnemyMovementStep step = route.movementSequence[index];
        if (!IsValid(step, enemy))
            return;

        enemy.Tracking.StartStep(step);
    }

    private static bool IsValid(EnemyMovementStep step, EnemyController enemy)
    {
        switch (step.movementType)
        {
            case EnemyMovementType.Idle when step.idlePoint == null:
                Debug.LogWarning($"{enemy.name} has an Idle step with no idle point", enemy);
                return false;

            case EnemyMovementType.Patrol when step.patrolPath.wayPoints == null || step.patrolPath.wayPoints.Length == 0:
                Debug.LogWarning($"{enemy.name} has a Patrol step with no waypoints", enemy);
                return false;

            default:
                return true;
        }
    }
}
