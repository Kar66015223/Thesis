using UnityEngine;

namespace EditModeTests
{
    /// <summary>
    /// Plain C# stand-in for a world interactable (door, vault...). It only needs a GameObject
    /// for its position, which is all <see cref="PlayerInteractionHandler"/> reads from it.
    /// </summary>
    public class FakeInteractable : IInteractable
    {
        public FakeInteractable(GameObject owner, InteractButton button)
        {
            Owner = owner;
            InteractButton = button;
        }

        public GameObject Owner { get; }
        public InteractButton InteractButton { get; }

        public bool Interactable { get; set; } = true;
        public int InteractCount { get; private set; }

        public bool CanInteract(GameObject interactor) => Interactable;
        public void Interact(GameObject interactor) => InteractCount++;
    }
}
