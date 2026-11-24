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

    public virtual void Eat(GameObject owner)
    {
        if (!owner.TryGetComponent(out PlayerController player))
            return;

        InventoryModel inventory = player.Inventory;
        for(int i = 0; i < inventory.Slots.Count; i++)
        {
            if (inventory.Slots[i].Item != this)
                continue;

            inventory.RemoveOne(i);

            if (owner.TryGetComponent(out PlayerHunger hunger) && this is FoodData food)
                hunger.FillHunger(food.fillAmount);

            return;
        }
    }
    
    public virtual void Drop(GameObject owner)
    {
        Debug.Log("Drop");
    }
}
