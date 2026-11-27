using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public enum ItemType
{
    Usable,
    Valuable,
    Readable
}

public enum ItemAction
{
    Use,
    Eat,
    Drop
}

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    [TextArea] public string itemDesc = "Test.............................................................................";
    public Sprite icon;
    public ItemType itemType;

    [Min(1)] public int maxStack = 1;
    public float weight = 0.1f;
    public List<ItemAction> actionButtons = new();

    [Min(0)] public int sellPrice = 0;

    public virtual void Use()
    {
        Debug.Log("Use");
    }

    public virtual void Eat(GameObject owner, int slotIndex)
    {
        if (!TryTakenFrom(owner, slotIndex, out _))
            return;

        if (owner.TryGetComponent(out PlayerHunger hunger) && this is FoodData food)
            hunger.FillHunger(food.fillAmount);
    }

    public virtual void Drop(GameObject owner, int slotIndex)
    {
        if (TryTakenFrom(owner, slotIndex, out PlayerController player))
            player.DropItemObject(this);
    }
    
    private bool TryTakenFrom(GameObject owner, int slotIndex, out PlayerController player)
    {
        if (!owner.TryGetComponent(out player))
            return false;

        InventoryModel inventory = player.Inventory;
        if (slotIndex < 0 || slotIndex >= inventory.Slots.Count || inventory.Slots[slotIndex].Item != this)
            return false;

        inventory.RemoveOne(slotIndex);
        return true;
    }
}
