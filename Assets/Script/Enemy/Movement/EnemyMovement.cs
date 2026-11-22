using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    private readonly Dictionary<string, float> speedModifiers = new();
    private NavMeshAgent agent;
    private float baseSpeed;
    private float speedFloor;

    private NavMeshAgent Agent => agent != null ? agent : agent = GetComponent<NavMeshAgent>();

    public void SetBaseSpeed(float speed)
    {
        baseSpeed = speed;
        ApplySpeed();
    }

    public void SetSpeedFloor(float speed)
    {
        speedFloor = speed;
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

        Agent.speed = Mathf.Max(baseSpeed * multiplier, speedFloor);
    }
}
