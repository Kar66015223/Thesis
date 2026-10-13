using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Inventory Detail")]
    public Inventory inventory;

    [Header("Slot Detail")]
    public ItemData item;
    public int stack;

    [Header("UI")]
    public Color emtyColor = Color.white;
    public Color itemColor = Color.white;
    public Image icon;
    public TextMeshProUGUI stackText;

    [Header("Drag and Drop")]
    public int siblingIndex;
    public RectTransform draggable;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        siblingIndex = transform.GetSiblingIndex();
        RefreshUI();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (inventory == null || IsEmpty()) return;

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
        if (inventory == null || IsEmpty() || draggable == null || canvas == null) return;
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
        if (source == null || source == this || source.inventory != inventory || source.IsEmpty()) return;

        if (item == source.item)
        {
            int maxStack = Mathf.Max(1, item.maxStack);
            int room = Mathf.Max(0, maxStack - stack);
            int moved = Mathf.Min(room, source.stack);
            if (moved <= 0) return;

            SetThisSlot(item, stack + moved);
            source.SetThisSlot(source.item, source.stack - moved);
        }
        else
        {
            ItemData oldItem = item;
            int oldStack = stack;
            SetThisSlot(source.item, source.stack);
            source.SetThisSlot(oldItem, oldStack);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right || inventory == null || IsEmpty()) return;

        inventory.SetRightClickSlot(this);
        inventory.OpenMiniCanvas(eventData.position);
    }

    public void UseItem()
    {
        if (IsEmpty() || stack <= 0) return;

        stack--;
        if (stack <= 0)
            SetThisSlot(inventory != null ? inventory.EMPTY_ITEM : null, 0);
        else
            RefreshUI();
    }

    public void SwapSlot(InventorySlot newSlot)
    {
        if (newSlot == null || newSlot == this) return;
        ItemData oldItem = item;
        int oldStack = stack;
        SetThisSlot(newSlot.item, newSlot.stack);
        newSlot.SetThisSlot(oldItem, oldStack);
    }

    public void SetSwap(ItemData swapItem, int amount)
    {
        SetThisSlot(swapItem, amount);
    }

    public void MergeThisSlot(InventorySlot mergeSlot)
    {
        if (mergeSlot == null || mergeSlot == this || mergeSlot.IsEmpty()) return;
        if (IsEmpty() || item != mergeSlot.item)
        {
            SwapSlot(mergeSlot);
            return;
        }

        int room = Mathf.Max(0, Mathf.Max(1, item.maxStack) - stack);
        int moved = Mathf.Min(room, mergeSlot.stack);
        if (moved <= 0) return;
        SetThisSlot(item, stack + moved);
        mergeSlot.SetThisSlot(mergeSlot.item, mergeSlot.stack - moved);
    }

    public void MergeThisSlot(ItemData mergeItem, int mergeAmount)
    {
        if (inventory != null && !inventory.IsEmptyItem(mergeItem) && mergeAmount > 0)
            inventory.AddItem(mergeItem, mergeAmount);
    }

    public void SetThisSlot(ItemData newItem, int amount)
    {
        if (inventory != null && inventory.IsEmptyItem(newItem))
        {
            item = inventory.EMPTY_ITEM;
            stack = 0;
        }
        else if (newItem == null || amount <= 0)
        {
            item = inventory != null ? inventory.EMPTY_ITEM : null;
            stack = 0;
        }
        else
        {
            item = newItem;
            stack = Mathf.Clamp(amount, 0, Mathf.Max(1, newItem.maxStack));
        }

        RefreshUI();
    }

    public void CheckShowText()
    {
        RefreshUI();
    }

    public void UpdateColorSlot()
    {
        bool empty = IsEmpty();
        if (icon != null)
        {
            icon.color = empty ? emtyColor : itemColor;
            icon.sprite = empty ? (item != null ? item.icon : null) : item.icon;
            icon.enabled = !empty || icon.sprite != null;
        }
    }

    private bool IsEmpty()
    {
        return inventory != null ? inventory.IsEmptyItem(item) : item == null;
    }

    private void RefreshUI()
    {
        UpdateColorSlot();
        if (stackText == null) return;

        stackText.text = stack.ToString();
        stackText.gameObject.SetActive(!IsEmpty() && item.maxStack > 1 && stack > 1);
    }
}
