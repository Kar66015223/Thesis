using TMPro;
using UnityEngine;

/// <summary>
/// Locking strategy for a <see cref="Door"/>. A lock is not an interactable by itself - the door asks
/// it whether the interactor may pass. Add a new lock type by subclassing; Door never changes.
/// </summary>
public abstract class Lock : MonoBehaviour
{
    [SerializeField] protected TMP_Text stateUIText;

    public Door CurrentDoor { get; private set; }
    public bool IsLocked { get; private set; } = true;

    protected virtual void Awake()
    {
        CurrentDoor = GetComponentInParent<Door>();
        RefreshStateText();
    }

    /// <summary>
    /// Called when the interactor tries to open a locked door.
    /// Returns true if the door is now unlocked and should open immediately.
    /// </summary>
    public abstract bool TryUnlock(GameObject interactor);

    public void ForceUnlock() => SetLocked(false);

    protected void SetLocked(bool locked)
    {
        IsLocked = locked;
        RefreshStateText();
    }

    /// <summary>True when the interactor stands on the given side of the door.</summary>
    protected bool IsOnSide(GameObject interactor, Transform side)
        => CurrentDoor != null && CurrentDoor.GetClosestPoint(interactor.transform.position) == side;

    private void RefreshStateText()
    {
        if (stateUIText != null)
            stateUIText.text = IsLocked ? "Locked" : "Unlocked";
    }
}
