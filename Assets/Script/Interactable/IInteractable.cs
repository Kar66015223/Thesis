using UnityEngine;

public interface IInteractable
{
    GameObject Owner { get; set; }

    bool CanInteract(GameObject interactor);
    void Interact(GameObject interactor);
}
