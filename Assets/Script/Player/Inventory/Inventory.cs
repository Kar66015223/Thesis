using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Grid view + context menu for an <see cref="InventoryModel"/>, opened and closed with the inventory key (Tab).
/// All item rules live in the model; this class only builds slot views and forwards clicks.
/// Sits on an always-active object (PlayerCanvas), not on the panel it hides, so it still hears the key while closed.
/// </summary>
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
    }

    void OnDisable()
    {
        if (inputHandler != null)
            inputHandler.OnInventoryPressed -= Toggle;

        InputModeService.ActionMapChanged -= HandleActionMapChanged;
        InputModeService.RequestUI(this, false);
    }

    void Start()
    {
        if (model == null)
        {
            PlayerController owner = GetComponentInParent<PlayerController>();
            Bind(owner != null ? owner.Inventory : new InventoryModel(slotAmount));
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

        // Don't open on top of another UI (dialogue, password pad, ...) or while struggling.
        // Checked against those maps, not the mission map, so it still opens in the hideout's Player_Base map.
        string currentMap = InputModeService.CurrentActionMap;
        if (isOpen && (currentMap == PlayerConstants.ACTIONMAP_UI || currentMap == PlayerConstants.ACTIONMAP_STRUGGLE))
            return;

        if (!isOpen)
            OnFinishMiniCanvas();

        panel.SetActive(isOpen);
        InputModeService.RequestUI(this, isOpen);
    }

    // Getting grabbed switches to the Struggle map, where the inventory key can't close the panel.
    // Watched on the map, not StruggleToggled: the struggle prompt only appears once the strangle loop starts.
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

    /// <summary>Removes the item from the slot without spawning it in the world.</summary>
    public void DestroyItem()
    {
        model?.ClearSlot(contextSlotIndex);
        OnFinishMiniCanvas();
    }

    // public void OpenMiniCanvas(int slotIndex, Vector2 screenPosition)
    // {
    //     if (miniCanvas == null) return;

    //     contextSlotIndex = slotIndex;
    //     miniCanvas.position = screenPosition;
    //     miniCanvas.gameObject.SetActive(true);
    // }

    public void OnFinishMiniCanvas()
    {
        contextSlotIndex = -1;
        if (miniCanvas != null)
            miniCanvas.gameObject.SetActive(false);
    }

    public void SetLayoutControlChild(bool isControlled)
    {
        if (gridLayoutGroup != null)
            gridLayoutGroup.enabled = isControlled;
    }

    private void BuildSlotViews()
    {
        if (inventoryPanel == null || slotPrefab == null)
        {
            Debug.LogError("Inventory: assign Slot Prefab and Inventory Panel in the Inspector.", this);
            return;
        }

        // Only remove old slots so other UI under the panel survives.
        foreach (Transform child in inventoryPanel)
        {
            if (child.GetComponent<InventorySlot>() != null)
                Destroy(child.gameObject);
        }

        gridLayoutGroup = inventoryPanel.GetComponent<GridLayoutGroup>();
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
                slotViews[i].Show(model.Slots[i]);
        }
    }
}
