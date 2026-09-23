using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SelectionHandler
{
    [SerializeField] private ScrollRect scrollRect;
    
    private Dictionary<Item, GameObject> allItemButtons = new();
    private List<Item> allItems = new();
    private int selectedIndex = 0;

    public void Initialize(Dictionary<Item, GameObject> allItemButtons)
    {
        this.allItemButtons = allItemButtons;
    }

    public void UpdateSelection(List<Item> currentInter)
    {
        allItems = currentInter;

        if (allItems.Count == 0)
            return;

        if (selectedIndex >= allItems.Count)
        {
            selectedIndex = allItems.Count - 1;
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        Item selectedItem = allItems[selectedIndex];

        foreach (var kvp in allItemButtons)
        {
            InteractItemUI itemUI = kvp.Value.GetComponent<InteractItemUI>();
            itemUI.isSelected = kvp.Key == selectedItem;
        }
        
        if(scrollRect != null && allItems.Count > 1)
        {
            float normalizedScroll = 1f - ((float)selectedIndex / (allItems.Count - 1));
            scrollRect.verticalNormalizedPosition = normalizedScroll;
        }
    }

    public void HandleScrollSelect()
    {
        if (allItems.Count <= 1)
            return;

        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            if (scroll > 0)
                selectedIndex--;
            else
                selectedIndex++;

            if (selectedIndex < 0)
                selectedIndex = allItems.Count - 1;
            else if (selectedIndex >= allItems.Count)
                selectedIndex = 0;

            UpdateUI();
        }
    }
    
    public Item GetSelectedItem()
    {
        if (allItems.Count == 0)
            return null;

        return allItems[selectedIndex];
    }
}