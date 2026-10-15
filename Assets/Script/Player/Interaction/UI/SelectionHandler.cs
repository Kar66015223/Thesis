using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Highlights the selected item button and keeps it scrolled into view.</summary>
[System.Serializable]
public class SelectionHandler
{
    [SerializeField] private ScrollRect scrollRect;

    private int lastIndex = -1;
    private int lastCount = -1;

    public void Render(IReadOnlyDictionary<Item, InteractItemUI> buttons, IReadOnlyList<Item> items, int selectedIndex)
    {
        Item selectedItem = items.Count > 0 ? items[selectedIndex] : null;

        foreach (var pair in buttons)
        {
            if (pair.Value != null)
                pair.Value.SetSelected(pair.Key == selectedItem);
        }

        // Only scroll when the selection changes, so the player can still scroll the list manually.
        bool selectionChanged = selectedIndex != lastIndex || items.Count != lastCount;
        if (scrollRect != null && items.Count > 1 && selectionChanged)
            scrollRect.verticalNormalizedPosition = 1f - (float)selectedIndex / (items.Count - 1);

        lastIndex = selectedIndex;
        lastCount = items.Count;
    }
}
