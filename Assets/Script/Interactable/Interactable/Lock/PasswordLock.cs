using UnityEngine;

public class PasswordLock : Lock
{
    [SerializeField] private string correctPassword = "1111";
    private GameObject interactor;

    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);
        this.interactor = interactor;
        GameEvent.OnShowPasswordInputUI?.Invoke(true, this);
    }

    public void Unlock()
    {
        IsLocked = false;
        CurrentDoor.Interact(interactor);
    }

    public string GetCorrectPassword() => correctPassword;
}