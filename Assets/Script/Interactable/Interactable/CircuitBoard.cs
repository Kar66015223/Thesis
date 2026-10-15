using UnityEngine;

public class CircuitBoard : Interactable
{
    protected override void OnInteract(GameObject interactor)
    {
        WorldEvents.SetLightsOut(true);
        UIEvents.ShowTip("Lights out, enemies come to check circuit board");
        EnemyEvents.RequestAdvanceAllRoutes();
    }
}
