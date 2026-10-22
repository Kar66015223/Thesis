using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [SerializeField] private ObjectiveHighlight highlight;

    public GameObject Owner => gameObject;
    public virtual InteractButton InteractButton => InteractButton.F;

    public virtual string Prompt => "";

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