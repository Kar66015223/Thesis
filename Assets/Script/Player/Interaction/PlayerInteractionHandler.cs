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

    public void PerformInteract()
    {
        if (selectedInteractable != null && selectedInteractable.CanInteract())
            selectedInteractable.Interact(player);
    }

    public void SetSelected(IInteractable interactable)
    {
        selectedInteractable = interactable;
        selectedObj = selectedInteractable?.Owner;
    }
}