using UnityEngine;

public class MissionNPC : Interactable
{
    protected override void OnInteract(GameObject interactor) => UIEvents.ToggleMissionBoard(true);
}