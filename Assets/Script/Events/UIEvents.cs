using System;
using UnityEngine;

/// <summary>
/// Requests that only the UI layer reacts to.
/// Events can only be raised through the methods below, so no one can accidentally
/// overwrite the subscriber list (which a public static Action field allowed).
/// </summary>
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
}
