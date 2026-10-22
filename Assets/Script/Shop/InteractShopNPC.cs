using UnityEngine;

/// <summary>
/// Opens the sell shop, the same way <see cref="MissionNPC"/> opens the mission board. Deriving from Interactable
/// (instead of implementing IInteractable directly) gives it the interact prompt and objective highlight.
/// </summary>
public class InteractShopNPC : Interactable
{
    protected override void OnInteract(GameObject interactor) => UIEvents.ToggleShopBoard(true);
}
