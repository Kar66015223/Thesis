using UnityEngine;

public enum EnemyReaction
{
    LookAt,
    WalkTo,
    RunTo
}

public class SoundSignal
{
    public Vector3 Position { get; private set; }
    public float Radius { get; private set; }
    public EnemyReaction Reaction { get; private set; }

    public SoundSignal(Vector3 position, float radius, EnemyReaction reaction)
    {
        Position = position;
        Radius = radius;
        Reaction = reaction;
    }
}
