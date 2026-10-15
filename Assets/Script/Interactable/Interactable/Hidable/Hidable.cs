using UnityEngine;

/// <summary>A spot the interactor can hide in. Works with anything that implements <see cref="IHidingAgent"/>.</summary>
public abstract class Hidable : Interactable
{
    [SerializeField] private Transform enterPos;
    [SerializeField] private Transform exitPos;

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
        }
        else if (occupant == agent)
        {
            occupant = null;
            agent.ExitHiding(exitPos.position);
        }
    }
}
