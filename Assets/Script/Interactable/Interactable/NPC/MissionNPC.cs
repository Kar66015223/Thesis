using UnityEngine;

public class MissionNPC : Interactable
{
    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);
        GameEvent.OnToggleUIQuestNPC?.Invoke(true);
    }
}