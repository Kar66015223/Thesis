using UnityEngine;

public class QuestNPC : Interactable
{
    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);
        GameEvent.OnToggleUIQuestNPC?.Invoke(true);
    }
}