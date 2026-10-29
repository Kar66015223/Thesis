using UnityEngine;

public class InteractShopNPC : Interactable
{
    protected override void OnInteract(GameObject interactor) => UIEvents.ToggleShopBoard(true);
}
