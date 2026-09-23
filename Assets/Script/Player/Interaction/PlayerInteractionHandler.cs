using UnityEngine;

[System.Serializable]
public class PlayerInteractionHandler
{
    private GameObject player;
    
    private Item selectedItem;
    private Interactable selectedInteractable;
    public void Initialize(GameObject player)
    {
        this.player = player;
    }

    public void PerformFInteract()
    {
        if (selectedItem != null && selectedItem.CanInteract(player))
            selectedItem.Interact(player);
    }

    public void PerformSpacebarInteract()
    {
        if (selectedInteractable != null && selectedInteractable.CanInteract(player))
            selectedInteractable.Interact(player);
    }

    public void SetSelectedItem(Item item) => selectedItem = item;
    public void SetSelectedInteractable(Interactable interact) => selectedInteractable = interact;
}