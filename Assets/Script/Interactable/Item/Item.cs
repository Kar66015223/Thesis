using UnityEngine;

public class Item : InteractableBase
{
    [field: SerializeField] public ItemData Data { get; private set; }
    [SerializeField] private AudioClip pickUpClip;
    
    protected sealed override void OnInteract(GameObject interactor)
    {
        if (Data != null && !(Data as ReadableItemData) && interactor.TryGetComponent(out IItemHolder holder)
            && holder.AddItem(Data) && interactor.TryGetComponent(out PlayerController player))
            player.StoreItemObject(this);

        WorldEvents.RaiseItemCollected(this);
        OnPickedUp(interactor);
    }
    
    protected virtual void OnPickedUp(GameObject interactor)
    {
        gameObject.SetActive(false);
    }
}