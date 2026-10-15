using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Grid view + context menu for an <see cref="InventoryModel"/>.
/// All item rules live in the model; this class only builds slot views and forwards clicks.
/// </summary>
public class Inventory : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private InventorySlot slotPrefab;
    [SerializeField] private Transform inventoryPanel;
    [Tooltip("Only used when no PlayerController is found to provide the model.")]
    [SerializeField, Min(1)] private int slotAmount = 30;

    [Header("Mini Canvas")]
    [SerializeField] private RectTransform miniCanvas;

    private InventoryModel model;
    private InventorySlot[] slotViews = new InventorySlot[0];
    private GridLayoutGroup gridLayoutGroup;
    private int contextSlotIndex = -1;

    public InventoryModel Model => model;

    void Start()
    {
        if (model == null)
        {
            PlayerController owner = GetComponentInParent<PlayerController>();
            Bind(owner != null ? owner.Inventory : new InventoryModel(slotAmount));
        }

        if (miniCanvas != null)
            miniCanvas.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (model != null)
            model.Changed -= Refresh;
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
