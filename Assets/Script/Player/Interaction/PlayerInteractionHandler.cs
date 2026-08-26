using UnityEngine;

public class PlayerInteractionHandler : MonoBehaviour
{
    [SerializeField] private IInteractable selectedInteractable;

    public void PerformInteract()
    {
        if (selectedInteractable != null && selectedInteractable.CanInteract())
            selectedInteractable.Interact();
    }
}