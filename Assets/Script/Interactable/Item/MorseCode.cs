using UnityEngine;

public class MorseCode : Item
{
    [SerializeField] private ObjectiveData objTiveData; 
    protected override void OnPickedUp(GameObject interactor)
    {
        base.OnPickedUp(interactor);
        UIEvents.ShowTip("There it is, now I just need to get out of here.");
        UIEvents.RequestChangeObjective(objTiveData);
        WorldEvents.SetLightsOut(false);
        EnemyEvents.RequestAdvanceAllRoutes();
    }
}