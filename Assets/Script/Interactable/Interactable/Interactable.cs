using UnityEngine;

public abstract class Interactable : InteractableBase, IPromptTarget
{
    [field: SerializeField] public Transform InteractPromptPos { get; private set; }
    public Transform PromptAnchor => InteractPromptPos;
}