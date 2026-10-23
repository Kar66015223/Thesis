using UnityEngine;

/// <summary>The key itself is stored by the base Item pickup; this only adds the hint.</summary>
public class KeyForBasement : Item
{
    protected override void OnPickedUp(GameObject interactor)
    {
        base.OnPickedUp(interactor);
        UIEvents.ShowTip("This key is for power room, there's might be something useful there.");
        WorldEvents.RaiseObjectiveShow(true); // <-- Check true if player has this in inventory instead later.
    }
}