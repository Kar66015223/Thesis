using UnityEngine;

public class KeyForBasement : Item
{
    protected override void OnPickedUp(GameObject interactor)
    {
        base.OnPickedUp(interactor);
        UIEvents.ShowTip("This key is for power room, there's might be something useful there.");
        WorldEvents.RaiseObjectiveShow(true);
    }
}