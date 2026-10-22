using System;
using UnityEngine;

public static class UIEvents
{
    public static event Action<string> TipRequested;
    public static void ShowTip(string message) => TipRequested?.Invoke(message);

    public static event Action<bool, string> ReadPanelToggled;
    public static void ToggleReadPanel(bool isOpen, string text = "") => ReadPanelToggled?.Invoke(isOpen, text);

    public static event Action<bool, IPasswordLock> PasswordPanelToggled;
    public static void TogglePasswordPanel(bool isOpen, IPasswordLock target = null)
        => PasswordPanelToggled?.Invoke(isOpen, target);

    public static event Action<bool> MissionBoardToggled;
    public static void ToggleMissionBoard(bool isOpen) => MissionBoardToggled?.Invoke(isOpen);

    /// <summary>Only one tracked target at a time; pass null to clear.</summary>
    public static event Action<Transform, Sprite> TrackingTargetChanged;
    public static void SetTrackingTarget(Transform target, Sprite icon)
        => TrackingTargetChanged?.Invoke(target, icon);

    public static event Action<DialogueData> DialogueRequested;
    public static void RequestDialogue(DialogueData data) => DialogueRequested?.Invoke(data);

    public static event Action<bool> ShopBoardToggled;
    public static void ToggleShopBoard(bool isOpen) => ShopBoardToggled?.Invoke(isOpen);
}