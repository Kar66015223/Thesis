using UnityEngine;

/// <summary>A world object used through a floating "[F]" prompt (doors, vaults, NPCs...).</summary>
public abstract class Interactable : InteractableBase, IPromptTarget
{
    [field: SerializeField] public Transform InteractPromptPos { get; private set; }

    public Transform PromptAnchor => InteractPromptPos;
}
