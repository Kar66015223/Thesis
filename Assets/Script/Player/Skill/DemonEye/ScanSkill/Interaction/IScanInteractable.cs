using UnityEngine;

public interface IScanInteractable
{
    GameObject Owner { get; }
    bool CanInteract();
    void Interact();
}