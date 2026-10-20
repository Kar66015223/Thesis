using UnityEngine;

/// <summary>The key itself is stored by the base Item pickup; this only adds the hint.</summary>
public class KeyForBasement : Item
{
    protected override void OnPickedUp(GameObject interactor)
    {
        // base.OnPickedUp(interactor);
        UIEvents.ShowTip("Player has key, can unlock basement door now");
    }
}