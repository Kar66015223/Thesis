using UnityEngine;

/// <summary>Tiny reusable timer so every skill doesn't hand-roll "isCooldown / currentCooldown" fields.</summary>
public class Cooldown
{
    private float remaining;

    public bool IsReady => remaining <= 0f;
    public float Remaining => Mathf.Max(0f, remaining);

    public void Start(float duration) => remaining = duration;

    public void Tick(float deltaTime)
    {
        if (remaining > 0f)
            remaining -= deltaTime;
    }
}
