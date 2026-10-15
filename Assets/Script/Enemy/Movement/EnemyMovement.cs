using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Final agent speed = base speed (set by the current state) x every active multiplier
/// (route step, future slow/haste effects...). Each source owns its own key, so they never overwrite each other.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    private readonly Dictionary<string, float> speedModifiers = new();
    private NavMeshAgent agent;
    private float baseSpeed;

    private NavMeshAgent Agent => agent != null ? agent : agent = GetComponent<NavMeshAgent>();

    public void SetBaseSpeed(float speed)
    {
        baseSpeed = speed;
        ApplySpeed();
    }

    public void SetSpeedModifier(string source, float multiplier)
    {
        speedModifiers[source] = multiplier;
        ApplySpeed();
    }

    public void RemoveSpeedModifier(string source)
    {
        if (speedModifiers.Remove(source))
            ApplySpeed();
    }

    private void ApplySpeed()
    {
        float multiplier = 1f;
        foreach (float m in speedModifiers.Values)
            multiplier *= m;

        Agent.speed = baseSpeed * multiplier;
    }
}
