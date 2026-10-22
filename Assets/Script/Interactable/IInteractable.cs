using UnityEngine;

public interface IInteractable
{
    GameObject Owner { get; }
    InteractButton InteractButton { get; }
    string Prompt { get; }

    bool CanInteract(GameObject interactor);
    void Interact(GameObject interactor);
}
