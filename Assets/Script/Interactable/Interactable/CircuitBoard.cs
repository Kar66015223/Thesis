using UnityEngine;

public class CircuitBoard : Interactable
{
    [SerializeField] private ObjectiveData objTiveData;
    private bool hasChangedPath = false;
    
    protected override void OnInteract(GameObject interactor)
    {
        WorldEvents.SetLightsOut(true);
        WorldEvents.RaiseObjectiveShow(true);
        UIEvents.ShowTip("Someone will come to check, I need to hide.");
        UIEvents.RequestChangeObjective(objTiveData);

        if (!hasChangedPath)
        {
            EnemyEvents.RequestAdvanceAllRoutes();
            hasChangedPath = true;
        }
    }
}
