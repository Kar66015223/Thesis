using UnityEngine;

public class Item : InteractableBase
{
    [field: SerializeField] public ItemData Data { get; private set; }
    private bool GoesInInventory => Data != null && !(Data is ReadableItemData);

    public bool CanBeTakenBy(GameObject interactor)
        => !GoesInInventory || !interactor.TryGetComponent(out IItemHolder holder) || holder.CanAdd(Data);

    protected sealed override void OnInteract(GameObject interactor)
    {
        if(GoesInInventory && interactor.TryGetComponent(out IItemHolder holder))
        {
            if (!holder.AddItem(Data))
                return;

            if (interactor.TryGetComponent(out PlayerController player))
                player.StoreItemObject(this);
        }

        WorldEvents.RaiseItemCollected(this);
        OnPickedUp(interactor);
    }
    
    protected virtual void OnPickedUp(GameObject interactor)
    {
        gameObject.SetActive(false);
        if(!PlayerSession.firstItemTaken)
        {
            UIEvents.ShowTip("[Tab] Open inventory");
            PlayerSession.firstItemTaken = true;
        }
    }
}