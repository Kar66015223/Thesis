using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [Header("Inventory")]
    [Tooltip("Root shown and hidden by the inventory key.")]
    [SerializeField] private GameObject panel;
    [SerializeField] private InventorySlot slotPrefab;
    [Tooltip("Grid the slots are spawned into.")]
    [SerializeField] private Transform inventoryPanel;
    [Tooltip("Only used when no PlayerController is found to provide the model.")]
    [SerializeField, Min(1)] private int slotAmount = 30;
    [SerializeField, Min(0.1f)] private float maxWeight = 50f;

    [SerializeField] private TMP_Text weightText;

    private InventorySlot selectedSlot;

    [Header("Item Detail")]
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemDescText;

    [SerializeField] private GameObject actionButtonPrefab;
    [SerializeField] private Transform actionButtonsParent;
    private List<GameObject> spawnedActionButtons = new();

    [Header("Sort Buttons")]
    [SerializeField] private Button sortAscendingButton;
    [SerializeField] private Button sortDescendingButton;

    [Header("Mini Canvas")]
    [SerializeField] private RectTransform miniCanvas;
    [SerializeField] private Button useButton;
    [SerializeField] private Button destroyButton;

    private InventoryModel model;
    private InventorySlot[] slotViews = new InventorySlot[0];
    private GridLayoutGroup gridLayoutGroup;
    private ContentSizeFitter contentSizeFitter;
    private PlayerInputHandler inputHandler;
    private int contextSlotIndex = -1;

    public InventoryModel Model => model;
    public bool IsOpen => panel != null && panel.activeSelf;

    void Awake()
    {
        inputHandler = GetComponentInParent<PlayerInputHandler>();

        if (sortAscendingButton != null)
            sortAscendingButton.onClick.AddListener(() => SortItem(true));
        if (sortDescendingButton != null)
            sortDescendingButton.onClick.AddListener(() => SortItem(false));
        if (useButton != null)
            useButton.onClick.AddListener(UseItem);
        if (destroyButton != null)
            destroyButton.onClick.AddListener(DestroyItem);
    }

    void OnEnable()
    {
        if (inputHandler != null)
            inputHandler.OnInventoryPressed += Toggle;

        InputModeService.ActionMapChanged += HandleActionMapChanged;
        UIEvents.SlotSelectRequest += RefreshSelected;
    }

    void OnDisable()
    {
        if (inputHandler != null)
            inputHandler.OnInventoryPressed -= Toggle;

        InputModeService.ActionMapChanged -= HandleActionMapChanged;
        InputModeService.RequestUI(this, false);

        UIEvents.SlotSelectRequest -= RefreshSelected;
    }

    void Start()
    {
        if (model == null)
        {
            PlayerController owner = GetComponentInParent<PlayerController>();
            Bind(owner != null ? owner.Inventory : new InventoryModel(slotAmount, maxWeight));
        }

        if (miniCanvas != null)
            miniCanvas.gameObject.SetActive(false);

        if (panel != null)
            panel.SetActive(false);
    }

    void OnDestroy()
    {
        if (model != null)
            model.Changed -= Refresh;
    }

    public void Toggle() => SetOpen(!IsOpen);

    public void SetOpen(bool isOpen)
    {
        if (panel == null || isOpen == IsOpen)
            return;

        string currentMap = InputModeService.CurrentActionMap;
        if (isOpen && (currentMap == PlayerConstants.ACTIONMAP_UI || currentMap == PlayerConstants.ACTIONMAP_STRUGGLE))
            return;

        if (!isOpen)
            OnFinishMiniCanvas();

        panel.SetActive(isOpen);
        InputModeService.RequestUI(this, isOpen);
    }

    private void HandleActionMapChanged(string actionMap)
    {
        if (actionMap == PlayerConstants.ACTIONMAP_STRUGGLE)
            SetOpen(false);
    }

    public void Bind(InventoryModel newModel)
    {
        if (model != null)
            model.Changed -= Refresh;

        model = newModel;
        if (model == null)
            return;

        BuildSlotViews();
        model.Changed += Refresh;
        Refresh();
    }

    public void MoveItem(int fromIndex, int toIndex) => model?.Move(fromIndex, toIndex);
    public void SortItem(bool ascending = true) => model?.Sort(ascending);

    public void UseItem()
    {
        model?.UseOne(contextSlotIndex);
        OnFinishMiniCanvas();
    }

    public void DestroyItem()
    {
        model?.ClearSlot(contextSlotIndex);
        OnFinishMiniCanvas();
    }

    public void OpenMiniCanvas(int slotIndex, Vector2 screenPosition)
    {
        if (miniCanvas == null) return;

        contextSlotIndex = slotIndex;
        miniCanvas.position = screenPosition;
        miniCanvas.gameObject.SetActive(true);
    }

    public void OnFinishMiniCanvas()
    {
        contextSlotIndex = -1;
        if (miniCanvas != null)
            miniCanvas.gameObject.SetActive(false);
    }

    public void SetLayoutControlChild(bool isControlled)
    {
        if (!isControlled && contentSizeFitter != null)
            contentSizeFitter.enabled = false;

        if (gridLayoutGroup != null)
            gridLayoutGroup.enabled = isControlled;

        if (isControlled && contentSizeFitter != null)
            contentSizeFitter.enabled = true;
    }

    private void BuildSlotViews()
    {
        if (inventoryPanel == null || slotPrefab == null)
        {
            Debug.LogError("Inventory: assign Slot Prefab and Inventory Panel in the Inspector.", this);
            return;
        }

        foreach (Transform child in inventoryPanel)
        {
            if (child.GetComponent<InventorySlot>() != null)
                Destroy(child.gameObject);
        }

        gridLayoutGroup = inventoryPanel.GetComponentInChildren<GridLayoutGroup>();
        contentSizeFitter = inventoryPanel.GetComponentInChildren<ContentSizeFitter>();
        slotViews = new InventorySlot[model.Capacity];

        for (int i = 0; i < slotViews.Length; i++)
        {
            slotViews[i] = Instantiate(slotPrefab, inventoryPanel);
            slotViews[i].Initialize(this, i);
        }
    }

    private void Refresh()
    {
        for (int i = 0; i < slotViews.Length; i++)
        {
            if (slotViews[i] != null)
            {
                slotViews[i].Show(model.Slots[i]);
            }
        }

        if (model.CurrentWeight > model.MaxWeight)
            weightText.SetText($"Weight: <color=red>{model.CurrentWeight}</color> / {model.MaxWeight}");
        else
            weightText.SetText($"Weight: {model.CurrentWeight} / {model.MaxWeight}");
    }

    private void RefreshSelected(InventorySlot selected)
    {
        foreach (InventorySlot slot in slotViews)
        {
            if (slot == null)
                continue;

            if (selected == slot)
                selectedSlot = slot;
        }

        if (selectedSlot == null)
            return;

        itemNameText.text = selectedSlot.ItemName;
        itemDescText.text = selectedSlot.ItemDesc;

        ShowActionButtons(GetItemAt(selectedSlot.Index));
    }

    private ItemData GetItemAt(int index)
    {
        if (model == null || index < 0 || index >= model.Capacity)
            return null;

        InventoryModel.Slot slot = model.Slots[index];
        return slot.IsEmpty ? null : slot.Item;
    }

    private void ShowActionButtons(ItemData item)
    {
        ClearActionButtons();

        if (item == null || item.actionButtons == null || actionButtonPrefab == null || actionButtonsParent == null)
            return;

        for (int i = 0; i < item.actionButtons.Count; i++)
        {
            GameObject button = Instantiate(actionButtonPrefab, actionButtonsParent);

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                label.text = item.actionButtons[i];

            spawnedActionButtons.Add(button);
        }
    }

    private void ClearActionButtons()
    {
        foreach (GameObject button in spawnedActionButtons)
        {
            if (button != null)
                Destroy(button);
        }

        spawnedActionButtons.Clear();
    }
}
