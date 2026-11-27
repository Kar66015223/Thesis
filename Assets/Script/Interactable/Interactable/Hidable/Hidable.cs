using Unity.VisualScripting;
using UnityEngine;

public abstract class Hidable : Interactable
{
    public override string Prompt => "Hide";

    [SerializeField] private Transform enterPos;
    [SerializeField] private Transform exitPos;

    [SerializeField] private AudioClip enterSound;
    [SerializeField] private AudioClip exitSound;

    private IHidingAgent occupant;

    public bool IsHide { get; protected set; } = false;
    public bool IsOccupied => occupant != null;
    protected bool isExiting;
    protected virtual bool CanExit => IsHide && !isExiting;

    protected virtual bool CrouchOnEnter => false;
    protected virtual Quaternion? EnterFacing => null;

    public override bool CanInteract(GameObject interactor) => base.CanInteract(interactor) && !IsOccupied;
    protected override void OnInteract(GameObject interactor) => ToggleHide(interactor);
    
    public void ToggleHide(GameObject interactor)
    {
        if (!interactor.TryGetComponent(out IHidingAgent agent))
            return;

        if (occupant == null)
        {
            occupant = agent;
            if (CrouchOnEnter)
                agent.SetCrouching(true);
            agent.EnterHiding(this, enterPos.position, EnterFacing);
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

    protected virtual void BeginExit() => FinishExit(exitPos.position);

    protected void FinishExit(Vector3 position, Quaternion? facing = null)
    {
        if (occupant == null)
            return;

        IHidingAgent agent = occupant;
        occupant = null;
        agent.ExitHiding(position, facing);
        OnExitHiding();
    }

    public void CancelHiding()
    {
        if (occupant == null)
            return;

        IHidingAgent agent = occupant;
        if (CrouchOnEnter)
            agent.SetCrouching(false);

        isExiting = false;

        if (TryGetComponent(out AudioSource source))
            source.Stop();

        FinishExit(exitPos.position);
        OnCancelHiding();
        IsHide = false;
    }
    
    protected virtual void OnCancelHiding() {}

    protected virtual void OnEnterHiding() { }

    protected virtual void OnExitHiding() { }

    public void SetHide(bool isHide) => IsHide = isHide && IsOccupied;
}
