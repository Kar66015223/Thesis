using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryModel : IItemHolder
{
    public class Slot
    {
        public ItemData Item { get; private set; }
        public int Stack { get; internal set; }
        public bool IsEmpty => Item == null || Stack <= 0;

        internal void Set(ItemData item, int stack)
        {
            if (item == null || stack <= 0)
            {
                Item = null;
                Stack = 0;
                return;
            }

            Item = item;
            Stack = Mathf.Clamp(stack, 0, MaxStackOf(item));
        }
    }

    private readonly Slot[] slots;

    public event Action Changed;
    public event Action<ItemData> ItemAdded;

    public int Capacity => slots.Length;
    public IReadOnlyList<Slot> Slots => slots;

    public InventoryModel(int capacity)
    {
        slots = new Slot[Mathf.Max(1, capacity)];
        for (int i = 0; i < slots.Length; i++)
            slots[i] = new Slot();
    }

    public static int MaxStackOf(ItemData item) => item == null ? 0 : Mathf.Max(1, item.maxStack);
    public bool HasItem(ItemData item) => item != null && slots.Any(s => s.Item == item && s.Stack > 0);

    public bool CanAdd(ItemData item, int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        int maxStack = MaxStackOf(item);
        int space = 0;
        foreach (Slot slot in slots)
        {
            if (slot.IsEmpty)
                space += maxStack;
            else if (slot.Item == item)
                space += Mathf.Max(0, maxStack - slot.Stack);
        }

        return space >= amount;
    }

    public bool AddItem(ItemData item, int amount = 1)
    {
        if (!CanAdd(item, amount))
            return false;

        int maxStack = MaxStackOf(item);
        int remaining = amount;

        foreach (Slot slot in slots)
        {
            if (remaining == 0)
                break;
            if (slot.IsEmpty || slot.Item != item)
                continue;

            int added = Mathf.Min(maxStack - slot.Stack, remaining);
            slot.Set(item, slot.Stack + added);
            remaining -= added;
        }

        foreach (Slot slot in slots)
        {
            if (remaining == 0)
                break;
            if (!slot.IsEmpty)
                continue;

            int added = Mathf.Min(maxStack, remaining);
            slot.Set(item, added);
            remaining -= added;
        }

        ItemAdded?.Invoke(item);
        Changed?.Invoke();

        return true;
    }

    public void ClearSlot(int index)
    {
        if (!IsValid(index))
            return;

        slots[index].Set(null, 0);
        Changed?.Invoke();
    }

    public void UseOne(int index)
    {
        if (!IsValid(index) || slots[index].IsEmpty)
            return;

        Slot slot = slots[index];
        slot.Set(slot.Item, slot.Stack - 1);
        Changed?.Invoke();
    }

    public void Move(int from, int to)
    {
        if (!IsValid(from) || !IsValid(to) || from == to || slots[from].IsEmpty)
            return;

        Slot source = slots[from];
        Slot target = slots[to];

        if (target.Item == source.Item)
        {
            int moved = Mathf.Min(MaxStackOf(target.Item) - target.Stack, source.Stack);
            if (moved <= 0)
                return;

            target.Set(target.Item, target.Stack + moved);
            source.Set(source.Item, source.Stack - moved);
        }
        else
        {
            ItemData oldItem = target.Item;
            int oldStack = target.Stack;
            target.Set(source.Item, source.Stack);
            source.Set(oldItem, oldStack);
        }

        Changed?.Invoke();
    }

    public void Sort(bool ascending = true)
    {
        var entries = slots
            .Where(s => !s.IsEmpty)
            .GroupBy(s => s.Item)
            .Select(g => (item: g.Key, amount: g.Sum(s => s.Stack)))
            .ToList();

        entries = ascending
            ? entries.OrderBy(e => e.item.itemName, StringComparer.Ordinal).ToList()
            : entries.OrderByDescending(e => e.item.itemName, StringComparer.Ordinal).ToList();

        foreach (Slot slot in slots)
            slot.Set(null, 0);

        int index = 0;
        foreach (var (item, amount) in entries)
        {
            int remaining = amount;
            while (remaining > 0 && index < slots.Length)
            {
                int placed = Mathf.Min(MaxStackOf(item), remaining);
                slots[index++].Set(item, placed);
                remaining -= placed;
            }
        }

        Changed?.Invoke();
    }

    public (ItemData item, int stack)[] Snapshot() => slots.Select(s => (s.Item, s.Stack)).ToArray();

    public void Restore(IReadOnlyList<(ItemData item, int stack)> contents)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (contents != null && i < contents.Count)
                slots[i].Set(contents[i].item, contents[i].stack);
            else
                slots[i].Set(null, 0);
        }

        Changed?.Invoke();
    }

    private bool IsValid(int index) => index >= 0 && index < slots.Length;
}