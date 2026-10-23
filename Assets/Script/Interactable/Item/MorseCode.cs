using UnityEngine;

/// <summary>Mission 1 goal item: picking it up restores power and sends enemies to their next route step.</summary>
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