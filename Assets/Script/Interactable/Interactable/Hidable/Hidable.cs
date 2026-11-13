using UnityEngine;

public abstract class Hidable : Interactable
{
    public override string Prompt => "Hide";

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
            OnEnterHiding(agent);
        }
        else if(occupant == agent)
        {
            occupant = null;
            agent.ExitHiding(exitPos.position);
            OnExitHiding(agent);
        }
    }

    /// <summary>เรียกหลังผู้เล่นเข้าไปซ่อน (override เพื่อเพิ่มเสียง/เอฟเฟกต์)</summary>
    protected virtual void OnEnterHiding(IHidingAgent agent) { }

    /// <summary>เรียกหลังผู้เล่นออกจากที่ซ่อน (override เพื่อเพิ่มเสียง/เอฟเฟกต์)</summary>
    protected virtual void OnExitHiding(IHidingAgent agent) { }
}
