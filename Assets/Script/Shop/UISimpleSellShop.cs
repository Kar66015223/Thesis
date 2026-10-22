using UnityEngine;

using UnityEngine.UI;

using TMPro;



public class UISimpleSellShop : MonoBehaviour

{

    [Header("Shop UI")]

    [SerializeField] private GameObject rootPanel;

    [SerializeField] private TMP_Text playerMoneyText;

    [SerializeField] private Button closeButton;



    [Header("Sell List")]

    [SerializeField] private Transform contentParent;

    [SerializeField] private GameObject sellItemPrefab;



    private InventoryModel playerInventory;



    void Awake()

    {

        if (closeButton != null)

            closeButton.onClick.AddListener(() => UIEvents.ToggleShopBoard(false));

    }



    void OnEnable()

    {

        UIEvents.ShopBoardToggled += OnShopToggled;

        if (EconomyManager.Instance != null)

            EconomyManager.Instance.MoneyChanged += UpdateMoneyUI;

    }



    void OnDisable()

    {

        UIEvents.ShopBoardToggled -= OnShopToggled;

        if (EconomyManager.Instance != null)

            EconomyManager.Instance.MoneyChanged -= UpdateMoneyUI;



        InputModeService.RequestUI(this, false);

    }



    void Start()

    {

        // หากระเป๋าของผู้เล่น

        PlayerController player = FindObjectOfType<PlayerController>();

        if (player != null) playerInventory = player.Inventory;



        rootPanel.SetActive(false);

    }



    private void OnShopToggled(bool isOpen)

    {

        rootPanel.SetActive(isOpen);

        InputModeService.RequestUI(this, isOpen); // ล็อคหน้าจอ/เมาส์



        if (isOpen)

        {

            UpdateMoneyUI(EconomyManager.Instance != null ? EconomyManager.Instance.CurrentMoney : 0);

            RefreshSellList();

        }

    }



    private void RefreshSellList()

    {

        // 1. ลบปุ่มเก่าทิ้งทั้งหมด

        foreach (Transform child in contentParent) Destroy(child.gameObject);



        if (playerInventory == null) return;



        // 2. เช็คของในกระเป๋าทีละช่อง

        for (int i = 0; i < playerInventory.Slots.Count; i++)

        {

            var slot = playerInventory.Slots[i];

            if (!slot.IsEmpty)

            {

                CreateItemUI(slot.Item, slot.Stack, i);

            }

        }

    }



    private void CreateItemUI(ItemData item, int amount, int slotIndex)

    {

        GameObject go = Instantiate(sellItemPrefab, contentParent);



        // *สำคัญ: ชื่อข้างใน (Icon, NameText, ฯลฯ) ต้องตรงกับที่ตั้งชื่อใน Unity เป๊ะๆ*

        Image iconImage = go.transform.Find("Icon").GetComponent<Image>();

        TMP_Text nameText = go.transform.Find("NameText").GetComponent<TMP_Text>();

        TMP_Text priceText = go.transform.Find("PriceText").GetComponent<TMP_Text>();

        Button sellBtn = go.transform.Find("SellButton").GetComponent<Button>();



        iconImage.sprite = item.icon;

        nameText.text = $"{item.itemName} (x{amount})";

        priceText.text = $"Price: {item.sellPrice * amount} G"; // คำนวณราคาเหมา



        sellBtn.onClick.AddListener(() => SellItem(item, amount, slotIndex));

    }



    private void SellItem(ItemData item, int amount, int slotIndex)

    {

        // เช็คให้ชัวร์ว่าของยังอยู่

        if (playerInventory.Slots[slotIndex].IsEmpty || playerInventory.Slots[slotIndex].Item != item)

        {

            RefreshSellList(); return;

        }



        // เพิ่มเงิน -> ลบของ -> รีเฟรชหน้าจอ

        if (EconomyManager.Instance != null) EconomyManager.Instance.AddMoney(item.sellPrice * amount);

        playerInventory.ClearSlot(slotIndex);

        RefreshSellList();

    }



    private void UpdateMoneyUI(int money)

    {

        if (playerMoneyText != null) playerMoneyText.text = $"Money: {money} G";

    }

}