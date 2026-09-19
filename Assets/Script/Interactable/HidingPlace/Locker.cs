using UnityEngine;

public class Locker : HidingPlace, IInteractable
{
    public GameObject Owner { get; set; }

    void Awake()
        => Owner = gameObject;

    public bool CanInteract()
    {
        return true;
    }

    public void Interact(GameObject interactor)
    {
        ToggleHide(interactor);
    }
}
