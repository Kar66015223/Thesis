using System;

public static class GameEvent
{
    public static Action<bool, string> OnToggleReadUI;
    public static Action<string> OnShowTips;
    public static Action<bool, PasswordLock> OnShowPasswordInputUI;
    public static Action<bool> OnLightsOut;
}