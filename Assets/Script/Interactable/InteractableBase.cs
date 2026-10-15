using UnityEngine;

/// <summary>
/// Shared base for everything the player can use (world interactables and pick-up items).
/// <see cref="Interact"/> is a template method: the shared steps (availability check, objective
/// progress) always run, and subclasses only implement <see cref="OnInteract"/>, so no subclass
/// can forget to call base.Interact().
/// Whether the *player* is allowed to interact (running, hiding...) is the player's decision,
/// not the object's, so this class has no dependency on PlayerController.
/// </summary>
public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [SerializeField] private ObjectiveHighlight highlight;

    public GameObject Owner => gameObject;
    public virtual InteractButton InteractButton => InteractButton.F;

    public virtual bool CanInteract(GameObject interactor) => isActiveAndEnabled;

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor))
            return;

        if (highlight != null)
            highlight.ReportInteraction();

        OnInteract(interactor);
    }

    protected abstract void OnInteract(GameObject interactor);
}
