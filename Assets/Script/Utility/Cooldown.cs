using UnityEngine;

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
