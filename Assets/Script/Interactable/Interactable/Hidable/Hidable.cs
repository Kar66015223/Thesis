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
    protected virtual bool CanExit => true;

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
            OnEnterHiding();

            if (TryGetComponent(out AudioSource source) && enterSound != null)
                source.PlayOneShot(enterSound);
        }
        else if(occupant == agent && CanExit)
        {
            if (TryGetComponent(out AudioSource source) && exitSound != null)
                source.PlayOneShot(exitSound);

            BeginExit();
        }
    }

    /// <summary>
    /// Starts leaving. By default the player comes out straight away at Exit Pos.
    /// Override to play an animation first, then call <see cref="FinishExit"/> when it is done.
    /// </summary>
    protected virtual void BeginExit() => FinishExit(exitPos.position);

    /// <summary>Brings the player out of hiding at the given spot, optionally facing a given way.</summary>
    protected void FinishExit(Vector3 position, Quaternion? facing = null)
    {
        if (occupant == null)
            return;

        IHidingAgent agent = occupant;
        occupant = null;
        agent.ExitHiding(position, facing);
        OnExitHiding();
    }

    /// <summary>Called right after the player has gone into this hiding place.</summary>
    protected virtual void OnEnterHiding() { }

    /// <summary>Called right after the player has come out of this hiding place.</summary>
    protected virtual void OnExitHiding() { }
}
