using UnityEngine.InputSystem;

/// <summary>
/// The keys that advance dialogue, read through the Input System.
/// Kept in one place so it can later be swapped for an action in the UI action map.
/// </summary>
public static class DialogueInput
{
    public static bool ClickPressed => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    public static bool ContinuePressed
    {
        get
        {
            Keyboard keyboard = Keyboard.current;
            
            return keyboard != null &&
                (keyboard.spaceKey.wasPressedThisFrame ||
                keyboard.enterKey.wasPressedThisFrame ||
                keyboard.numpadEnterKey.wasPressedThisFrame);
        }
    }
    public static bool AnyHeld
    {
        get
        {
            Keyboard keyboard = Keyboard.current;
            bool mouseHeld = Mouse.current != null && Mouse.current.leftButton.isPressed;
            bool keyHeld = keyboard != null &&
                (keyboard.spaceKey.isPressed || keyboard.enterKey.isPressed || keyboard.numpadEnterKey.isPressed);

            return mouseHeld || keyHeld;
        }
    }
}