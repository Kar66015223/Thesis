using UnityEngine;

public class KeyForBasement : Item
{
    protected override void OnPickedUp(GameObject interactor)
    {
        base.OnPickedUp(interactor);
        WorldEvents.RaiseObjectiveShow(true);
    }
}