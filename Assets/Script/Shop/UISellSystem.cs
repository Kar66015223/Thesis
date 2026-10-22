using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISellSystem : MonoBehaviour
{
    public static UISellSystem Instance { get; private set; }

    [Header("Core References")]
    [SerializeField] private GameObject rootPanel; // หน้าต่าง UI ทั้งหมด (Inventory + Sell)
    [SerializeField] private Inventory playerInventoryUI; // UI กระเป๋าฝั่งซ้าย
    [SerializeField] private Inventory sellCartUI; // UI ตะกร้าขายฝั่งขวา

    [Header("Popup Menus")]
    [SerializeField] private RectTransform inventoryPopupMenu; // เมนู "ขาย / ยกเลิก"
    [SerializeField] private RectTransform sellPopupMenu;      // เมนู "เอากลับ / ยกเลิก"
    [SerializeField] private Button sellBtn, cancelInvBtn;
    [SerializeField] private Button takeBackBtn, cancelSellBtn;

    [Header("Action Buttons")]
    [SerializeField] private Button confirmSellAllBtn; // ปุ่ม "ยืนยันขาย"
    [SerializeField] private Button cancelAllBtn;      // ปุ่ม "ยกเลิกขาย"
    [SerializeField] private TMP_Text totalMoneyText;  // แสดงยอดเงินรวมที่จะได้

    private InventoryModel playerModel;
    private InventoryModel sellModel;
    private int currentSelectedIndex = -1;
    private bool isSelectingFromPlayer = true;

    void Awake()
    {
        Instance = this;

        // ซ่อน Popup ไว้ก่อน
        ClosePopups();

        // ตั้งค่าปุ่มใน Popup
        sellBtn.onClick.AddListener(OnClickSell);
        cancelInvBtn.onClick.AddListener(ClosePopups);
        takeBackBtn.onClick.AddListener(OnClickTakeBack);
        cancelSellBtn.onClick.AddListener(ClosePopups);

        // ตั้งค่าปุ่มหลัก
        confirmSellAllBtn.onClick.AddListener(ConfirmSellAll);
        cancelAllBtn.onClick.AddListener(CancelAllAndClose);
    }

    void Start()
    {
        // สร้าง Model ตะกร้าขายของ (สมมติมี 15 ช่อง)
        sellModel = new InventoryModel(15);
        sellCartUI.Bind(sellModel);
        sellModel.Changed += UpdateTotalMoneyPreview;

        // ผูก Model ของผู้เล่น
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            playerModel = player.Inventory;
            playerInventoryUI.Bind(playerModel);
        }

        rootPanel.SetActive(false);
    }

    // ฟังก์ชันเปิด/ปิดหน้าร้าน
    public void ToggleShop(bool isOpen)
    {
        rootPanel.SetActive(isOpen);
        InputModeService.RequestUI(this, isOpen);

        if (isOpen)
        {
            ClosePopups();
            UpdateTotalMoneyPreview();
        }
        else
        {
            CancelAllAndClose(); // ถ้าปิดหน้าต่าง ให้คืนของเข้าตัวอัตโนมัติ
        }
    }

    // --- ระบบย้ายไอเทมข้ามกระเป๋า (ใช้สำหรับการลาก Drag & Drop ด้วย) ---
    public void TransferItem(InventoryModel source, int sourceIndex, InventoryModel dest, int destIndex = -1)
    {
        if (source.Slots[sourceIndex].IsEmpty) return;

        ItemData item = source.Slots[sourceIndex].Item;
        int amount = source.Slots[sourceIndex].Stack;

        // เช็คว่าปลายทางมีที่ว่างไหม
        if (dest.CanAdd(item, amount))
        {
            dest.AddItem(item, amount);
            source.ClearSlot(sourceIndex);
            ClosePopups();
        }
    }

    // --- ระบบ Popup (ทำงานเมื่อผู้เล่นคลิกที่ช่อง) ---
    public void OpenMenuForSlot(bool isPlayerInventory, int index, Vector2 screenPos)
    {
        ClosePopups();
        currentSelectedIndex = index;
        isSelectingFromPlayer = isPlayerInventory;

        if (isPlayerInventory)
        {
            if (playerModel.Slots[index].IsEmpty) return;
            inventoryPopupMenu.position = screenPos;
            inventoryPopupMenu.gameObject.SetActive(true);
        }
        else
        {
            if (sellModel.Slots[index].IsEmpty) return;
            sellPopupMenu.position = screenPos;
            sellPopupMenu.gameObject.SetActive(true);
        }
    }

    private void OnClickSell()
    {
        TransferItem(playerModel, currentSelectedIndex, sellModel);
    }

    private void OnClickTakeBack()
    {
        TransferItem(sellModel, currentSelectedIndex, playerModel);
    }

    private void ClosePopups()
    {
        inventoryPopupMenu.gameObject.SetActive(false);
        sellPopupMenu.gameObject.SetActive(false);
        currentSelectedIndex = -1;
    }

    // --- ระบบยืนยัน / ยกเลิก ทั้งหมด ---
    private void UpdateTotalMoneyPreview()
    {
        if (totalMoneyText == null) return;

        int total = 0;
        foreach (var slot in sellModel.Slots)
        {
            if (!slot.IsEmpty)
                total += slot.Item.sellPrice * slot.Stack;
        }
        totalMoneyText.text = $"Total: {total} G";
    }

    private void ConfirmSellAll()
    {
        int totalEarned = 0;
        for (int i = 0; i < sellModel.Slots.Count; i++)
        {
            var slot = sellModel.Slots[i];
            if (!slot.IsEmpty)
            {
                totalEarned += slot.Item.sellPrice * slot.Stack;
                sellModel.ClearSlot(i); // ลบของทิ้งเพราะขายแล้ว
            }
        }

        if (totalEarned > 0)
        {
            EconomyManager.Instance.AddMoney(totalEarned);
            Debug.Log($"ขายสำเร็จ ได้เงิน: {totalEarned} G");
        }
    }

    private void CancelAllAndClose()
    {
        // คืนของทุกชิ้นจากตะกร้าขาย กลับเข้ากระเป๋าผู้เล่น
        for (int i = 0; i < sellModel.Slots.Count; i++)
        {
            if (!sellModel.Slots[i].IsEmpty)
            {
                TransferItem(sellModel, i, playerModel);
            }
        }
        ClosePopups();
        rootPanel.SetActive(false);
        InputModeService.RequestUI(this, false);
    }
}