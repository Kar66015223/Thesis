using System;
using System.Collections.Generic;
using UnityEngine;

public static class InputModeService
{
    private static readonly HashSet<object> uiRequests = new();
    private static readonly HashSet<object> cursorRequests = new();
    private static bool isStruggling;

    public static event Action<string> ActionMapChanged;
    public static string CurrentActionMap { get; private set; }

    public static void RequestUI(object requester, bool active)
    {
        SetRequest(uiRequests, requester, active);
        Refresh();
    }

    public static void RequestCursor(object requester, bool visible)
    {
        SetRequest(cursorRequests, requester, visible);
        Refresh();
    }

    public static void SetStruggling(bool active)
    {
        isStruggling = active;
        Refresh();
    }

    public static void Refresh()
    {
        Prune(uiRequests);
        Prune(cursorRequests);

        bool cursorFree = uiRequests.Count > 0 || cursorRequests.Count > 0;
        Cursor.lockState = cursorFree ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = cursorFree;

        if (isStruggling)
            CurrentActionMap = PlayerConstants.ACTIONMAP_STRUGGLE;
        else if (uiRequests.Count > 0)
            CurrentActionMap = PlayerConstants.ACTIONMAP_UI;
        else
        {
            CurrentActionMap = MapManager.Instance != null ?
                MapManager.Instance.DefaultActionMapName : PlayerConstants.ACTIONMAP_PLAYER_MISSION;
        }

        ActionMapChanged?.Invoke(CurrentActionMap);
    }

    private static void SetRequest(HashSet<object> requests, object requester, bool active)
    {
        if (requester == null)
            return;

        if (active)
            requests.Add(requester);
        else
            requests.Remove(requester);
    }

    private static void Prune(HashSet<object> requests)
        => requests.RemoveWhere(r => r == null || (r is UnityEngine.Object unityObj && unityObj == null));
}