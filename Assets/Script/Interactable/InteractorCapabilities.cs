using UnityEngine;

public interface IInteractorBody
{
    void SetCanMove(bool canMove);
    void Teleport(Vector3 position, Quaternion? facing = null);
    void SetPhysicsEnabled(bool enabled);
}

public interface IHidingAgent
{
    bool IsHiding { get; }
    void EnterHiding(Hidable spot, Vector3 position);
    void ExitHiding(Vector3 position);
}

public interface IPromptTarget
{
    Transform PromptAnchor { get; }
}