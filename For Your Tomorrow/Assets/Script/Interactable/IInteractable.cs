using UnityEngine;

public interface IInteractable
{
    GameObject Owner { get; set; }

    bool CanInteract();
    void Interact();
}
