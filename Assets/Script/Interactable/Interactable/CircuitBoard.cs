using UnityEngine;

public class CircuitBoard : Interactable
{
    private bool hasChangedPath = false;
    
    protected override void OnInteract(GameObject interactor)
    {
        WorldEvents.SetLightsOut(true);
        WorldEvents.RaiseObjectiveShow(true);
        UIEvents.ShowTip("Someone's coming, I need to hide.");

        if (!hasChangedPath)
        {
            EnemyEvents.RequestAdvanceAllRoutes();
            hasChangedPath = true;
        }
    }
}
