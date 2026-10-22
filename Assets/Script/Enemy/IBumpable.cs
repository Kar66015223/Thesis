using UnityEngine;

/// <summary>Reacts when the player walks into it.</summary>
public interface IBumpable
{
    void OnBumped(Transform bumper);
}
