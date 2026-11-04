using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    , IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private Color emptyColor = Color.white;
    [SerializeField] private Color itemColor = Color.white;
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI stackText;

    private string itemName;
    public string ItemName => itemName;
    private string itemDesc;
    public string ItemDesc => itemDesc;

    [Header("Drag and Drop")]
    [SerializeField] private RectTransform draggable;

    private Inventory inventory;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private int siblingIndex;
    private bool isEmpty = true;
    private bool isSelected = false;
    public bool IsSelected => isSelected;

    public int Index { get; private set; }

    public void Initialize(Inventory owner, int index)
    {
        inventory = owner;
        Index = index;
    }

    void OnEnable() => UIEvents.SlotSelectRequest += OnSelectSlot;
    void OnDisable() => UIEvents.SlotSelectRequest -= OnSelectSlot;
    void OnDestroy() => UIEvents.SlotSelectRequest -= OnSelectSlot;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        siblingIndex = transform.GetSiblingIndex();
    }

    public void Show(InventoryModel.Slot slot)
    {
        isEmpty = slot.IsEmpty;

        itemName = isEmpty ? "" : slot.Item.itemName;
        itemDesc = isEmpty ? "" : slot.Item.itemDesc;

        if (icon != null)
        {
            icon.color = isEmpty ? emptyColor : itemColor;
            icon.sprite = isEmpty ? emptySprite : slot.Item.icon;
            icon.enabled = icon.sprite != null;
        }

        if (stackText != null)
        {
            stackText.text = slot.Stack.ToString();
            stackText.gameObject.SetActive(!isEmpty && slot.Item.maxStack > 1 && slot.Stack > 1);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (inventory == null || isEmpty) return;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
        }

        transform.SetAsLastSibling();
        inventory.SetLayoutControlChild(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (inventory == null || isEmpty || draggable == null || canvas == null) return;
        draggable.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }

        if (draggable != null)
            draggable.anchoredPosition = Vector2.zero;

        transform.SetSiblingIndex(siblingIndex);

        if (inventory != null)
            inventory.SetLayoutControlChild(true);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        InventorySlot source = eventData.pointerDrag.GetComponent<InventorySlot>();
        if (source == null || source == this || source.inventory != inventory) return;

        inventory.MoveItem(source.Index, Index);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && (inventory != null || !isEmpty))
            SetSelected(true);
            // inventory.OpenMiniCanvas(Index, eventData.position);
    }

    public void SetSelected(bool isSelected)
    {
        this.isSelected = isSelected;
        if (isSelected)
            UIEvents.RequestSlotSelect(this);
    }

    public void OnSelectSlot(InventorySlot slot)
    {
        if (slot == this)
            return;

        isSelected = false;
    }
}
