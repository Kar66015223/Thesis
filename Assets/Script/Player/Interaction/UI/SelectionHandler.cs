using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        bool selectionChanged = selectedIndex != lastIndex || items.Count != lastCount;
        if (scrollRect != null && items.Count > 1 && selectionChanged)
            scrollRect.verticalNormalizedPosition = 1f - (float)selectedIndex / (items.Count - 1);

        lastIndex = selectedIndex;
        lastCount = items.Count;
    }
}
