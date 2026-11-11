using UnityEngine;

public interface IScanInteractable
{
    GameObject Owner { get; }
    void CanInteract();
    void Interact();
}