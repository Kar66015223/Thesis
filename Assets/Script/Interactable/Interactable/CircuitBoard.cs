using UnityEngine;

public class CircuitBoard : Interactable
{
    protected override void OnInteract(GameObject interactor)
    {
        WorldEvents.SetLightsOut(true);
        UIEvents.ShowTip("Someone's coming, I need to hide.");
        EnemyEvents.RequestAdvanceAllRoutes();
    }
}
