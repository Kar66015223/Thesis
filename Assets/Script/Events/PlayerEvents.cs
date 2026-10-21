using System;
using UnityEngine;

/// <summary>Player state changes that other systems (mostly UI) observe.</summary>
public static class PlayerEvents
{
    public static event Action Died;
    public static void RaiseDied() => Died?.Invoke();

    /// <summary>The player came back at a checkpoint after dying.</summary>
    public static event Action Respawned;
    public static void RaiseRespawned() => Respawned?.Invoke();

    public static event Action<Transform> OnCheckpoint;
    public static void RaiseCheckpoint(Transform spawnPoint) => OnCheckpoint?.Invoke(spawnPoint);

    public static event Action<bool> StruggleToggled;
    public static void RaiseStruggleToggled(bool isStruggling) => StruggleToggled?.Invoke(isStruggling);

    /// <summary>Normalized 0..1 progress towards breaking free.</summary>
    public static event Action<float> StruggleProgressChanged;
    public static void RaiseStruggleProgressChanged(float progress) => StruggleProgressChanged?.Invoke(progress);
}