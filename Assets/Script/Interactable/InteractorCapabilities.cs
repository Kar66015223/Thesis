using UnityEngine;

public interface IInteractorBody
{
    void SetCanMove(bool canMove);
    void Teleport(Vector3 position, Quaternion? facing = null);
    void SetPhysicsEnabled(bool enabled);
    void SetInteracting(bool isInteracting);
}

public interface IHidingAgent
{
    bool IsHiding { get; }
    void EnterHiding(Hidable spot, Vector3 position, Quaternion? facing = null);
    void ExitHiding(Vector3 position, Quaternion? facing = null);
    void SetCrouching(bool crouching);
}

public interface IPromptTarget
{
    Transform PromptAnchor { get; }
}