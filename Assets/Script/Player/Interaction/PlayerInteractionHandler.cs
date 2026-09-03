using UnityEngine;

[System.Serializable]
public class PlayerInteractionHandler
{
    private IInteractable selectedInteractable;
    [SerializeField] private GameObject selectedObj;

    public void PerformInteract()
    {
        if (selectedInteractable != null && selectedInteractable.CanInteract())
            selectedInteractable.Interact();
    }

    public void SetSelected(IInteractable interactable)
    {
        selectedInteractable = interactable;
    }
}