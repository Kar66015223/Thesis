using System;
using UnityEngine;

public static class EnemyEvents
{
    /// <summary>
    /// The enemy's alert indicator changed. <paramref name="enemy"/> identifies the enemy,
    /// <paramref name="anchor"/> is where the icon should float.
    /// </summary>
    public static event Action<Transform, Transform, EnemyState> IndicatorChanged;
    public static void RaiseIndicatorChanged(Transform enemy, Transform anchor, EnemyState indicator)
        => IndicatorChanged?.Invoke(enemy, anchor, indicator);

    /// <summary>Normalized 0..1 "am I really seeing the player?" progress.</summary>
    public static event Action<Transform, float> ConfirmProgressChanged;
    public static void RaiseConfirmProgressChanged(Transform enemy, float progress)
        => ConfirmProgressChanged?.Invoke(enemy, progress);

    /// <summary>Level script asks every enemy to move on to the next step of its route.</summary>
    public static event Action AdvanceAllRoutesRequested;
    public static void RequestAdvanceAllRoutes() => AdvanceAllRoutesRequested?.Invoke();
}
