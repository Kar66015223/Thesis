using UnityEngine;

public class Locker : HidingPlace
{
    public override void Interact(GameObject interactor)
    {
        ToggleHide(interactor);
    }
}
