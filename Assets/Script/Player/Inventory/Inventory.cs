using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [Header("Inventory")]
    [Tooltip("ไอเทมตัวแทนช่องว่าง สามารถปล่อยว่างได้")]
    public ItemData EMPTY_ITEM;
    public Transform slotPrefab;
    public Transform InventoryPanel;
    [Min(1)] public int slotAmount = 30;
    public InventorySlot[] inventorySlots;

    [Header("Mini Canvas")]
    public RectTransform miniCanvas;
    [SerializeField] private InventorySlot rightClickSlot;

    private GridLayoutGroup gridLayoutGroup;

    private void Start()
    {
        if (InventoryPanel == null || slotPrefab == null)
        {
            Debug.LogError("Inventory: กรุณากำหนด Slot Prefab และ Inventory Panel ใน Inspector", this);
            return;
        }

        gridLayoutGroup = InventoryPanel.GetComponent<GridLayoutGroup>();
        CreateInventorySlots();
        if (miniCanvas != null)
            miniCanvas.gameObject.SetActive(false);
    }

    public bool IsEmptyItem(ItemData data)
    {
        return data == null || (EMPTY_ITEM != null && data == EMPTY_ITEM);
    }

    /// <summary>เพิ่มไอเทมทั้งหมดได้สำเร็จจึงคืน true; หากพื้นที่ไม่พอจะไม่เพิ่มบางส่วนและไม่ดรอป</summary>
    public bool AddItem(ItemData item, int amount = 1)
    {
        if (IsEmptyItem(item) || amount <= 0)
            return false;

        if (inventorySlots == null || inventorySlots.Length == 0)
        {
            Debug.LogWarning("Inventory ยังไม่ได้สร้างช่องเก็บของ", this);
            return false;
        }

        int spaceAvailable = 0;
        int maxStack = Mathf.Max(1, item.maxStack);

        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot == null) continue;

            if (slot.item == item)
                spaceAvailable += Mathf.Max(0, maxStack - slot.stack);
            else if (IsEmptyItem(slot.item))
                spaceAvailable += maxStack;
        }

        if (spaceAvailable < amount)
        {
            Debug.LogWarning($"Inventory เต็ม: ไม่สามารถเพิ่ม {item.itemName} x{amount} ได้", this);
            return false;
        }

        int remaining = amount;

        // เติมกองของไอเทมชนิดเดียวกันก่อน
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot == null || slot.item != item) continue;

            int room = Mathf.Max(0, maxStack - slot.stack);
            if (room <= 0) continue;

            int added = Mathf.Min(room, remaining);
            slot.SetThisSlot(item, slot.stack + added);
            remaining -= added;
            if (remaining == 0) return true;
        }

        // จากนั้นใช้ช่องว่าง
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot == null || !IsEmptyItem(slot.item)) continue;

            int added = Mathf.Min(maxStack, remaining);
            slot.SetThisSlot(item, added);
            remaining -= added;
            if (remaining == 0) return true;
        }

        return remaining == 0;
    }

    public void UseItem()
    {
        if (rightClickSlot == null) return;
        rightClickSlot.UseItem();
        OnFinishMiniCanvas();
    }

    // ลบไอเทมออกจากช่อง โดยไม่สร้างไอเทมในฉาก
    public void DestroyItem()
    {
        if (rightClickSlot == null) return;
        rightClickSlot.SetThisSlot(EMPTY_ITEM, 0);
        OnFinishMiniCanvas();
    }

    public void RemoveItem(InventorySlot slot)
    {
        if (slot != null)
            slot.SetThisSlot(EMPTY_ITEM, 0);
    }

    public void SortItem(bool Ascending = true)
    {
        if (inventorySlots == null) return;

        var items = inventorySlots
            .Where(slot => slot != null && !IsEmptyItem(slot.item) && slot.stack > 0)
            .Select(slot => new { Item = slot.item, Amount = slot.stack })
            .OrderBy(entry => entry.Item.itemName, Ascending ? System.StringComparer.Ordinal : System.StringComparer.Ordinal)
            .ToList();

        if (!Ascending)
            items = items.OrderByDescending(entry => entry.Item.itemName, System.StringComparer.Ordinal).ToList();

        foreach (InventorySlot slot in inventorySlots)
            if (slot != null) slot.SetThisSlot(EMPTY_ITEM, 0);

        foreach (var entry in items)
            AddItem(entry.Item, entry.Amount);
    }

    public void CreateInventorySlots()
    {
        if (InventoryPanel == null || slotPrefab == null) return;

        // ลบเฉพาะช่อง InventorySlot เดิม ป้องกันการลบ UI อื่นที่อยู่ใน Panel
        foreach (Transform child in InventoryPanel)
        {
            if (child.GetComponent<InventorySlot>() != null)
                Destroy(child.gameObject);
        }

        inventorySlots = new InventorySlot[Mathf.Max(1, slotAmount)];
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            Transform slotObject = Instantiate(slotPrefab, InventoryPanel);
            InventorySlot slot = slotObject.GetComponent<InventorySlot>();
            if (slot == null)
            {
                Debug.LogError("Slot Prefab ต้องมี InventorySlot component", slotObject);
                Destroy(slotObject.gameObject);
                continue;
            }

            inventorySlots[i] = slot;
            slot.inventory = this;
            slot.SetThisSlot(EMPTY_ITEM, 0);
        }
    }

    public InventorySlot IsEmptySlotLeft(ItemData itemChecker = null, InventorySlot itemSlot = null)
    {
        InventorySlot firstEmptySlot = null;

        if (inventorySlots == null) return null;

        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot == null || slot == itemSlot) continue;

            if (!IsEmptyItem(itemChecker) && slot.item == itemChecker && slot.stack < Mathf.Max(1, itemChecker.maxStack))
                return slot;

            if (IsEmptyItem(slot.item) && firstEmptySlot == null)
                firstEmptySlot = slot;
        }

        return firstEmptySlot;
    }

    public void SetLayoutControlChild(bool isControlled)
    {
        if (gridLayoutGroup == null && InventoryPanel != null)
            gridLayoutGroup = InventoryPanel.GetComponent<GridLayoutGroup>();
        if (gridLayoutGroup != null)
            gridLayoutGroup.enabled = isControlled;
    }

    public void SetRightClickSlot(InventorySlot slot)
    {
        rightClickSlot = slot;
    }

    public void OpenMiniCanvas(Vector2 clickPosition)
    {
        if (miniCanvas == null) return;
        miniCanvas.position = clickPosition;
        miniCanvas.gameObject.SetActive(true);
    }

    public void OnFinishMiniCanvas()
    {
        rightClickSlot = null;
        if (miniCanvas != null)
            miniCanvas.gameObject.SetActive(false);
    }
}
