using UnityEngine;

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
    
    protected virtual void OnPickedUp(GameObject interactor)
    {
        gameObject.SetActive(false);
    }
}