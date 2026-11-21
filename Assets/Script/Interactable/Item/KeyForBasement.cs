using System;
using UnityEngine;

public class KeyForBasement : Item
{
    [SerializeField] private ObjectiveData objTiveData;
    protected override void OnPickedUp(GameObject interactor)
    {
        base.OnPickedUp(interactor);
        WorldEvents.RaiseObjectiveShow(true);
        UIEvents.ShowTip("The metal door is locked by electricity. Maybe I can open it if I cut the power first?");
        UIEvents.RequestChangeObjective(objTiveData);
    }
}