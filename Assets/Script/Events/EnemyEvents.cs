using System;
using UnityEngine;

public static class EnemyEvents
{
    public static event Action<Transform, Transform, EnemyState> IndicatorChanged;
    public static void RaiseIndicatorChanged(Transform enemy, Transform anchor, EnemyState indicator)
        => IndicatorChanged?.Invoke(enemy, anchor, indicator);

    public static event Action<Transform, float> ConfirmProgressChanged;
    public static void RaiseConfirmProgressChanged(Transform enemy, float progress)
        => ConfirmProgressChanged?.Invoke(enemy, progress);

    public static event Action AdvanceAllRoutesRequested;
    public static void RequestAdvanceAllRoutes() => AdvanceAllRoutesRequested?.Invoke();

    public static event Action<EnemyController> AlertRequested;
    public static void RequestAlert(EnemyController enemy) => AlertRequested?.Invoke(enemy);

    public static event Action AlertAllRequested;
    public static void RequestAlertAll() => AlertAllRequested?.Invoke();
}
