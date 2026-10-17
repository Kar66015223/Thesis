using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Pure view: draws the pick-up list and the "[F]" prompt from what the handler decided.
/// It never changes the selection itself (it used to, which caused the item-selection bug).
/// </summary>
[System.Serializable]
public class PlayerInteractionUI
{
    [SerializeField] private GameObject listPanel;
    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private Transform itemButtonParent;
    [SerializeField] private TMP_Text interactPrompt;
    [SerializeField] private SelectionHandler selection = new();

    private readonly Dictionary<Item, InteractItemUI> itemButtons = new();
    private readonly List<Item> staleItems = new();
    private Camera cam;

    public void Initialize()
    {
        cam = Camera.main;
        HidePrompt();

        if (listPanel != null)
            listPanel.SetActive(false);
    }

    public void Render(PlayerInteractionHandler handler)
    {
        RenderItemList(handler.Items);
        selection.Render(itemButtons, handler.Items, handler.SelectedItemIndex);
        RenderPrompt(handler);
    }

    private void RenderItemList(IReadOnlyList<Item> items)
    {
        if (listPanel != null)
            listPanel.SetActive(items.Count > 0);

        staleItems.Clear();
        foreach (var pair in itemButtons)
        {
            if (!Contains(items, pair.Key))
                staleItems.Add(pair.Key);
        }

        foreach (Item stale in staleItems)
        {
            if (itemButtons[stale] != null)
                Object.Destroy(itemButtons[stale].gameObject);

            itemButtons.Remove(stale);
        }

        foreach (Item item in items)
        {
            if (!itemButtons.ContainsKey(item))
                itemButtons.Add(item, CreateItemButton(item));
        }
    }

    private InteractItemUI CreateItemButton(Item item)
    {
        GameObject button = Object.Instantiate(itemButtonPrefab, itemButtonParent);

        if (!button.TryGetComponent(out InteractItemUI itemUI))
            itemUI = button.AddComponent<InteractItemUI>();

        itemUI.SetLabel(item.Data != null ? item.Data.itemName : item.name);
        return itemUI;
    }

    private void RenderPrompt(PlayerInteractionHandler handler)
    {
        // Show the prompt for whatever F would use; if F goes to the item list, show Space's target instead.
        if (!TryShowPrompt(handler.GetTarget(InteractButton.F)) &&
            !TryShowPrompt(handler.GetTarget(InteractButton.Spacebar)))
        {
            HidePrompt();
        }
    }

    private bool TryShowPrompt(IInteractable target)
    {
        if (interactPrompt == null || !(target is IPromptTarget promptTarget) || promptTarget.PromptAnchor == null)
            return false;

        if (cam == null)
            cam = Camera.main;
        if (cam == null)
            return false;

        Vector3 screenPos = cam.WorldToScreenPoint(promptTarget.PromptAnchor.position);
        if (screenPos.z < 0f)
            return false;

        interactPrompt.text = $"[{target.InteractButton}]";
        interactPrompt.transform.position = screenPos;
        return true;
    }

    private void HidePrompt()
    {
        if (interactPrompt != null)
            interactPrompt.text = "";
    }

    private static bool Contains(IReadOnlyList<Item> items, Item item)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == item)
                return true;
        }

        return false;
    }
}
