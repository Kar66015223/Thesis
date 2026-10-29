using UnityEngine;

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

            return false;
        }

        if (!needKey)
        {
            return false;
        }

        if (HasKey(interactor))
            return Unlock();

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