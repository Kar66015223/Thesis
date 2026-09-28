using System;
using UnityEngine;

public static class GameEvent
{
    public static Action<bool, string> OnToggleReadUI;
    public static Action<string> OnShowTips;
    public static Action<bool, PasswordLock> OnShowPasswordInputUI;
    public static Action<bool> OnLightsOut;

    public static Action<Transform, EnemyState> OnAlertEnemyState;
}