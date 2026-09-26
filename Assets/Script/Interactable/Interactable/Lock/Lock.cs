using TMPro;
using UnityEngine;

public abstract class Lock : Interactable
{
    public Door CurrentDoor { get; private set; }
    public bool IsLocked { get; protected set; } = true;

    [SerializeField] protected TMP_Text stateUIText;

    protected override void Awake()
    {
        base.Awake();
        CurrentDoor = GetComponentInParent<Door>();
    }

    protected virtual void Update()
    {
        if (stateUIText != null)
            stateUIText.text = IsLocked ? "Locked" : "Unlocked";
    }

    public void ForceUnlock()
    {
        IsLocked = false;
    }
}