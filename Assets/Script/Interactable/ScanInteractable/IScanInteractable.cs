using UnityEngine;

public interface IScanInteractable
{
    GameObject Owner { get; }
    float HoldTime { get; }
    bool CanInteract();
    void Interact(GameObject interactor);
}