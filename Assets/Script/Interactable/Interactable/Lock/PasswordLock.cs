using UnityEngine;

public class PasswordLock : Lock, IPasswordLock
{
    [SerializeField] private string correctPassword = "1111";
    private GameObject waitingInteractor;

    public override bool TryUnlock(GameObject interactor)
    {
        waitingInteractor = interactor;
        UIEvents.TogglePasswordPanel(true, this);
        return false;
    }

    public bool TrySubmit(string code)
    {
        if (code != correctPassword)
            return false;

        SetLocked(false);

        if (waitingInteractor != null && CurrentDoor != null)
            CurrentDoor.Interact(waitingInteractor);

        waitingInteractor = null;
        return true;
    }
}