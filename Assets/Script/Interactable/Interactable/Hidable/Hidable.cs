using UnityEngine;

public abstract class Hidable : Interactable
{
    public override string Prompt => "Hide";

    [SerializeField] private Transform enterPos;
    [SerializeField] private Transform exitPos;

    [SerializeField] private AudioClip enterSound;
    [SerializeField] private AudioClip exitSound;

    private IHidingAgent occupant;

    public bool IsOccupied => occupant != null;

    public override bool CanInteract(GameObject interactor) => base.CanInteract(interactor) && !IsOccupied;
    protected override void OnInteract(GameObject interactor) => ToggleHide(interactor);
    
    public void ToggleHide(GameObject interactor)
    {
        if (!interactor.TryGetComponent(out IHidingAgent agent))
            return;

        if (occupant == null)
        {
            occupant = agent;
            agent.EnterHiding(this, enterPos.position);

            if (TryGetComponent(out AudioSource source) && enterSound != null)
                source.PlayOneShot(enterSound);
        }
        else if(occupant == agent)
        {
            occupant = null;
            agent.ExitHiding(exitPos.position);

            if (TryGetComponent(out AudioSource source) && exitSound != null)
                source.PlayOneShot(exitSound);
        }
    }
}
