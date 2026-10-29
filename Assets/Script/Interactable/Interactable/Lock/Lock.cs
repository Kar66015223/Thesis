using TMPro;
using UnityEngine;

public abstract class Lock : MonoBehaviour
{
    [SerializeField] protected TMP_Text stateUIText;

    public Door CurrentDoor{ get; private set; }
    public bool IsLocked { get; private set; } = true;

    protected virtual void Awake()
    {
        CurrentDoor = GetComponentInParent<Door>();
        RefreshStateText();
    }

    public abstract bool TryUnlock(GameObject interactor);

    public void ForceUnlock() => SetLocked(false);

    public void RestoreLocked(bool locked) => SetLocked(locked);

    protected void SetLocked(bool locked)
    {
        IsLocked = locked;
        RefreshStateText();
    }

    protected bool IsOnSide(GameObject interactor, Transform side)
        => CurrentDoor != null && CurrentDoor.GetClosestPoint(interactor.transform.position) == side;

    private void RefreshStateText()
    {
        if (stateUIText != null)
            stateUIText.text = IsLocked ? "Locked" : "Unlocked";
    }
}