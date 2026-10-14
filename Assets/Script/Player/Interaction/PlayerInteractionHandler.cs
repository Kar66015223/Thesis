using UnityEngine;

[System.Serializable]
public class PlayerInteractionHandler
{
    private GameObject player;
    private IInteractable selected;

    public void Initialize(GameObject player)
    {
        this.player = player;
    }
    
    public void PerformFInteract()
    {
        if (selected == null ||
            !selected.CanInteract(player) ||
            selected.InteractButton != InteractButton.F)
            return;

        selected.Interact(player);
    }

    public void PerformSpacebarInteract()
    {
        if (selected == null ||
            !selected.CanInteract(player) ||
            selected.InteractButton != InteractButton.Spacebar)
            return;

        selected.Interact(player);
    }

    public void SetSelected(IInteractable selected) => this.selected = selected;
}