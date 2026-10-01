using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyController))]
public class EnemyMovement : MonoBehaviour
{
    private EnemyController ctrl;
    
    private float baseSpeed;
    private readonly Dictionary<string, float> speedModifiers = new();

    void Awake()
    {
        ctrl = GetComponent<EnemyController>();
    }

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
        float mult = 1f;
        foreach (var m in speedModifiers.Values)
            mult *= m;

        ctrl.Agent.speed = baseSpeed * mult;
    }
}