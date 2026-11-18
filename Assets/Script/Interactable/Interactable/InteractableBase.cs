using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [SerializeField] private ObjectiveHighlight highlight;
    [SerializeField] private AudioClip interactSound;

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

        if (TryGetComponent(out AudioSource source) && interactSound != null)
            source.PlayOneShot(interactSound);
            
        OnInteract(interactor);
    }

    protected abstract void OnInteract(GameObject interactor);
}