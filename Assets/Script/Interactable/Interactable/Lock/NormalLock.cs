using UnityEngine;

/// <summary>
/// One-sided lock. From <see cref="unlockableSide"/> it opens freely (unless it needs a key);
/// from the other side it needs <see cref="requiredKey"/> when <see cref="needKey"/> is set.
/// </summary>
public class NormalLock : Lock
{
    [SerializeField] private Transform unlockableSide;
    [SerializeField] private bool needKey = false;

    [Tooltip("Item that opens this lock. If empty, the key from the scene's KeyForBasement pickup is used.")]
    [SerializeField] private ItemData requiredKey;

    public override bool TryUnlock(GameObject interactor)
    {
        bool onUnlockableSide = IsOnSide(interactor, unlockableSide);

        if (onUnlockableSide)
        {
            if (!needKey)
                return Unlock();

            UIEvents.ShowTip("Locked from the other side.");
            return false;
        }

        if (!needKey)
        {
            UIEvents.ShowTip("Can't open from this side.");
            return false;
        }

        if (HasKey(interactor))
            return Unlock();

        UIEvents.ShowTip("Door locked, need key.");
        return false;
    }

    private bool Unlock()
    {
        SetLocked(false);
        return true;
    }

    private bool HasKey(GameObject interactor)
    {
        ItemData key = ResolveRequiredKey();
        return key != null && interactor.TryGetComponent(out IItemHolder holder) && holder.HasItem(key);
    }

    // Keeps existing scenes working until requiredKey is assigned in the Inspector.
    private ItemData ResolveRequiredKey()
    {
        if (requiredKey != null)
            return requiredKey;

        KeyForBasement keyPickup = FindAnyObjectByType<KeyForBasement>(FindObjectsInactive.Include);
        if (keyPickup != null && keyPickup.Data != null)
        {
            requiredKey = keyPickup.Data;
            Debug.LogWarning($"{name}: 'Required Key' is not assigned, using '{requiredKey.name}' from the scene. " +
                "Assign it in the Inspector.", this);
        }

        return requiredKey;
    }
}