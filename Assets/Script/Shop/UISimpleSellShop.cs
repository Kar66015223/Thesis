using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISimpleSellShop : MonoBehaviour
{
    [Header("Shop UI")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private TMP_Text playerMoneyText;

    [SerializeField] private Button closeButton;
    [SerializeField] private Button removeAllButton;
    [SerializeField] private Button sellButton;

    [Header("Inventory Panel")]
    [SerializeField] private Transform inventoryContentParent;
    [SerializeField] private SellItemRow sellItemRowPrefab;

    [Header("Sell Panel")]
    [SerializeField] private Transform sellingContentParent;
    [SerializeField] private SellItemSlot sellItemSlotPrefab;

    private List<ItemData> itemsInSellingPanel = new();
    private List<ItemData> soldItems = new();

    private InventoryModel playerInventory;
    private EconomyManager economy;

    void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(() => UIEvents.ToggleShopBoard(false));
        if (removeAllButton != null)
            removeAllButton.onClick.AddListener(MoveAllSellingItems);
        if (sellButton != null)
            sellButton.onClick.AddListener(Sell);
    }

    void OnEnable() => UIEvents.ShopBoardToggled += SetOpen;

    void OnDisable()
    {
        UIEvents.ShopBoardToggled -= SetOpen;
        SetEconomy(null);
        InputModeService.RequestUI(this, false);
    }

    void Start()
    {
        PlayerController player = GetComponentInParent<PlayerController>();
        if (player == null)
            player = FindFirstObjectByType<PlayerController>();
        if (player != null)
            playerInventory = player.Inventory;

        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    private void SetOpen(bool isOpen)
    {
        if (rootPanel != null)
            rootPanel.SetActive(isOpen);

        InputModeService.RequestUI(this, isOpen);

        SetEconomy(isOpen ? EconomyManager.Instance : null);

        if (isOpen)
        {
            RefreshInventoryList();
            RefreshSellList();
        }
        else
            MoveAllSellingItems();
    }

    private void SetEconomy(EconomyManager newEconomy)
    {
        if (economy != null)
            economy.MoneyChanged -= UpdateMoneyText;

        economy = newEconomy;
        if (economy == null)
            return;

        economy.MoneyChanged += UpdateMoneyText;
        UpdateMoneyText(economy.CurrentMoney);
    }

    private void RefreshInventoryList()
    {
        foreach (Transform child in inventoryContentParent)
        {
            if (child.TryGetComponent(out SellItemRow _))
                Destroy(child.gameObject);
        }

        if (playerInventory == null || sellItemRowPrefab == null)
            return;

        for (int i = 0; i < playerInventory.Slots.Count; i++)
        {
            InventoryModel.Slot slot = playerInventory.Slots[i];
            if (slot.IsEmpty || slot.Item.sellPrice <= 0)
                continue;

            int slotIndex = i;
            ItemData item = slot.Item;
            int amount = slot.Stack;

            SellItemRow row = Instantiate(sellItemRowPrefab, inventoryContentParent);
            row.Bind(item, amount, () => MoveInventoryItem(slotIndex));
        }
    }

    private void RefreshSellList()
    {
        foreach (Transform child in sellingContentParent)
            if (child.TryGetComponent(out SellItemSlot _))
                Destroy(child.gameObject);

        if (itemsInSellingPanel.Count == 0 || sellItemSlotPrefab == null)
            return;

        for(int i = 0; i < itemsInSellingPanel.Count; i++)
        {
            int slotIndex = i;
            ItemData item = itemsInSellingPanel[i];

            SellItemSlot slot = Instantiate(sellItemSlotPrefab, sellingContentParent);
            slot.Bind(item, 1, () => MoveSellingItem(slotIndex));
        }
    }

    private void MoveInventoryItem(int slotIndex)
    {
        InventoryModel.Slot slot = playerInventory.Slots[slotIndex];
        itemsInSellingPanel.Add(slot.Item);
        playerInventory.ClearSlot(slotIndex);

        RefreshInventoryList();
        RefreshSellList();
    }

    private void MoveSellingItem(int slotIndex)
    {
        ItemData item = itemsInSellingPanel[slotIndex];
        playerInventory.AddItem(item);
        itemsInSellingPanel.RemoveAt(slotIndex);

        RefreshInventoryList();
        RefreshSellList();
    }
    
    private void MoveAllSellingItems()
    {
        if (itemsInSellingPanel.Count == 0)
            return;

        for (int i = 0; i < itemsInSellingPanel.Count; i++)
            playerInventory.AddItem(itemsInSellingPanel[i]);

        itemsInSellingPanel.Clear();
        RefreshInventoryList();
        RefreshSellList();
    }

    private void Sell()
    {
        if (economy == null)
        {
            Debug.LogWarning("UISimpleSellShop: no EconomyManager in the scene, so nothing can be sold.", this);
            return;
        }

        soldItems.Clear();
        foreach (ItemData item in itemsInSellingPanel)
        {
            soldItems.Add(item);
            economy.AddMoney(item.sellPrice);
        }

        foreach(ItemData sold in soldItems)
        {
            if (itemsInSellingPanel.Contains(sold))
                itemsInSellingPanel.Remove(sold);
        }
            
        RefreshInventoryList();
        RefreshSellList();
    }

    private void UpdateMoneyText(int money)
    {
        if (playerMoneyText != null)
            playerMoneyText.text = $"Money: {money} G";
    }
}
