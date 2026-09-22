using UnityEngine;

[System.Serializable]
public class PlayerInteractionHandler
{
    private GameObject player;
    
    private IInteractable selectedInteractable;
    [SerializeField] private GameObject selectedObj;

    public void Initialize(GameObject player)
    {
        this.player = player;
    }

    public void PerformFInteract()
    {
        if (selectedInteractable != null && selectedInteractable.CanInteract(player))
            if (selectedInteractable is Item item)
                item.Interact(player);
    }

    public void PerformSpacebarInteract()
    {
        if (selectedInteractable != null && selectedInteractable.CanInteract(player))
            if(selectedInteractable is Interactable interact)
                interact.Interact(player);
    }

    public void SetSelected(IInteractable interactable)
    {
        selectedInteractable = interactable;
        selectedObj = selectedInteractable?.Owner;
    }
}