using UnityEngine;

public static class PlayerSession
{
    private static (ItemData item, int stack)[] savedInventory;

    public static bool HasInventory => savedInventory != null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => savedInventory = null;

    public static void SaveInventory(InventoryModel inventory)
    {
        if (inventory != null)
            savedInventory = inventory.Snapshot();
    }

    public static void RestoreInventory(InventoryModel inventory)
    {
        if (inventory != null && savedInventory != null)
            inventory.Restore(savedInventory);
    }

    public static void Clear() => savedInventory = null;
}
