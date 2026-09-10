using UnityEngine;

public class SoundSignal
{
    public Vector3 Position { get; private set; }
    public float Radius { get; private set; }

    public SoundSignal(Vector3 position, float radius)
    {
        Position = position;
        Radius = radius;
    }
}
