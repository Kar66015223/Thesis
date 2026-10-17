using UnityEngine;

// Small, focused contracts that interactables use instead of reaching into PlayerController.
// Anything (player, NPC, test dummy) that implements them can use doors, vaults and hiding spots.

/// <summary>Physical control an interactable may need over whoever is using it.</summary>
public interface IInteractorBody
{
    void SetCanMove(bool canMove);
    void Teleport(Vector3 position, Quaternion? facing = null);
    /// <summary>Turns collision/physics movement on or off (used while being moved by a script).</summary>
    void SetPhysicsEnabled(bool enabled);
}

public interface IHidingAgent
{
    bool IsHiding { get; }
    void EnterHiding(Hidable spot, Vector3 position);
    void ExitHiding(Vector3 position);
}

/// <summary>An interactable that shows a world-space "[F]" style prompt.</summary>
public interface IPromptTarget
{
    Transform PromptAnchor { get; }
}