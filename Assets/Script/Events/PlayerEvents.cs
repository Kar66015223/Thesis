using System;

/// <summary>Player state changes that other systems (mostly UI) observe.</summary>
public static class PlayerEvents
{
    public static event Action Died;
    public static void RaiseDied() => Died?.Invoke();

    public static event Action<bool> StruggleToggled;
    public static void RaiseStruggleToggled(bool isStruggling) => StruggleToggled?.Invoke(isStruggling);

    /// <summary>Normalized 0..1 progress towards breaking free.</summary>
    public static event Action<float> StruggleProgressChanged;
    public static void RaiseStruggleProgressChanged(float progress) => StruggleProgressChanged?.Invoke(progress);
}