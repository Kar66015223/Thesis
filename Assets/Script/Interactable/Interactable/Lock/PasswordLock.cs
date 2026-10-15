using UnityEngine;

/// <summary>
/// Opens the password pad. The correct code never leaves this class: the UI submits a guess
/// through <see cref="IPasswordLock.TrySubmit"/>.
/// </summary>
public class PasswordLock : Lock, IPasswordLock
{
    [SerializeField] private string correctPassword = "1111";

    private GameObject waitingInteractor;

    public override bool TryUnlock(GameObject interactor)
    {
        waitingInteractor = interactor;
        UIEvents.TogglePasswordPanel(true, this);
        return false; // Opens later, once the right code is submitted.
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
