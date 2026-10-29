using UnityEngine;

public class MorseCode : Item
{
    protected override void OnPickedUp(GameObject interactor)
    {
        base.OnPickedUp(interactor);
        UIEvents.ShowTip("There it is, now I just need to get out of here.");
        WorldEvents.SetLightsOut(false);
        EnemyEvents.RequestAdvanceAllRoutes();
    }
}