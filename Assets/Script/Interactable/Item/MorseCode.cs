using UnityEngine;

/// <summary>Mission 1 goal item: picking it up restores power and sends enemies to their next route step.</summary>
public class MorseCode : Item
{
    protected override void OnPickedUp(GameObject interactor)
    {
        WorldEvents.SetLightsOut(false);
        EnemyEvents.RequestAdvanceAllRoutes();
    }
}