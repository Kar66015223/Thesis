using UnityEngine;

/// <summary>
/// A pick-up shown in the item list. Picking it up stores its <see cref="ItemData"/> in the
/// interactor's <see cref="IItemHolder"/>; subclasses add extra behaviour in <see cref="OnPickedUp"/>.
/// </summary>
public class Item : InteractableBase
{
    [field: SerializeField] public ItemData Data { get; private set; }

    protected sealed override void OnInteract(GameObject interactor)
    {
        if (Data != null && interactor.TryGetComponent(out IItemHolder holder))
            holder.AddItem(Data);

        WorldEvents.RaiseItemCollected(this);
        OnPickedUp(interactor);
    }
    
    protected virtual void OnPickedUp(GameObject interactor) {}
}