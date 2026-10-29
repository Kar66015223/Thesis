using System;
using UnityEngine;

public static class PlayerEvents
{
    public static event Action<bool> SlowCrouch;
    public static void RaiseSlowCrouch(bool isSlowCrouching) => SlowCrouch?.Invoke(isSlowCrouching);

    public static event Action Died;
    public static void RaiseDied() => Died?.Invoke();

    public static event Action Respawned;
    public static void RaiseRespawned() => Respawned?.Invoke();

    public static event Action<Transform> OnCheckpoint;
    public static void RaiseCheckpoint(Transform spawnPoint) => OnCheckpoint?.Invoke(spawnPoint);

    public static event Action<bool> StruggleToggled;
    public static void RaiseStruggleToggled(bool isStruggling) => StruggleToggled?.Invoke(isStruggling);

    public static event Action<float> StruggleProgressChanged;
    public static void RaiseStruggleProgressChanged(float progress) => StruggleProgressChanged?.Invoke(progress);
}