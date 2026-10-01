using System;
using UnityEngine;

public static class GameEvent
{
    public static Action<string> OnSwitchActionMap;

    // UI
    public static Action<bool, string> OnToggleReadUI;
    public static Action<string> OnShowTips;
    public static Action<bool, PasswordLock> OnShowPasswordInputUI;
    public static Action<bool> OnLightsOut;

    public static Action<Transform, EnemyState> OnAlertEnemyState;
    public static Action<Transform, float> OnUpdateConfirmTimer;
    public static Action<bool> OnToggleStruggleUI;

    public static Action<bool> OnToggleUIQuestNPC;

    // Enemy
    public static Action<EnemyController> OnChangeEnemyPath;
    public static Action OnChangeAllEnemyPath;
}