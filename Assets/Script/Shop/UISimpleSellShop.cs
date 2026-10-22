using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sell shop opened by <see cref="InteractShopNPC"/> or a dialogue choice: lists the player's inventory stacks and
/// sells a whole stack at <see cref="ItemData.sellPrice"/> each. Items priced 0 (keys, mission items) aren't listed.
/// Sits on the Player prefab's canvas, like the inventory.
/// </summary>
public class UISimpleSellShop : MonoBehaviour
{
    [Header("Shop UI")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private TMP_Text playerMoneyText;
    [SerializeField] private Button closeButton;

    [Header("Sell List")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private SellItemRow sellItemPrefab;

    private InventoryModel playerInventory;
    private EconomyManager economy;   // only set (and listened to) while the shop is open

    void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(() => UIEvents.ToggleShopBoard(false));
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
        // Same lookup as Inventory: the PlayerController above this canvas owns the inventory.
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

        // EconomyManager can wake up after this component (or come from another scene), so hook it up on open.
        SetEconomy(isOpen ? EconomyManager.Instance : null);

        if (isOpen)
            RefreshSellList();
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

    private void RefreshSellList()
    {
        // Only remove old rows so other UI under the list survives.
        foreach (Transform child in contentParent)
        {
            if (child.TryGetComponent(out SellItemRow _))
                Destroy(child.gameObject);
        }

        if (playerInventory == null || sellItemPrefab == null)
            return;

        for (int i = 0; i < playerInventory.Slots.Count; i++)
        {
            InventoryModel.Slot slot = playerInventory.Slots[i];
            if (slot.IsEmpty || slot.Item.sellPrice <= 0)
                continue;

            int slotIndex = i;
            ItemData item = slot.Item;
            int amount = slot.Stack;

            SellItemRow row = Instantiate(sellItemPrefab, contentParent);
            row.Bind(item, amount, () => SellSlot(slotIndex, item, amount));
        }
    }

    private void SellSlot(int slotIndex, ItemData item, int amount)
    {
        if (economy == null)
        {
            Debug.LogWarning("UISimpleSellShop: no EconomyManager in the scene, so nothing can be sold.", this);
            return;
        }

        // The slot may have changed since the list was built.
        InventoryModel.Slot slot = playerInventory.Slots[slotIndex];
        if (slot.IsEmpty || slot.Item != item || slot.Stack != amount)
        {
            RefreshSellList();
            return;
        }

        economy.AddMoney(item.sellPrice * amount);
        playerInventory.ClearSlot(slotIndex);
        RefreshSellList();
    }

    private void UpdateMoneyText(int money)
    {
        if (playerMoneyText != null)
            playerMoneyText.text = $"Money: {money} G";
    }
}
