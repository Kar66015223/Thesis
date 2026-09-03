using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SelectionHandler
{
    [SerializeField] private ScrollRect scrollRect;
    
    private Dictionary<IInteractable, GameObject> allItemButtons = new();
    private List<IInteractable> allInteractables = new();
    private int selectedIndex = 0;

    public void Initialize(Dictionary<IInteractable, GameObject> allItemButtons)
    {
        this.allItemButtons = allItemButtons;
    }

    public void UpdateSelection(List<IInteractable> currentInter)
    {
        allInteractables = currentInter;

        if (allInteractables.Count == 0)
            return;

        if (selectedIndex >= allInteractables.Count)
        {
            selectedIndex = allInteractables.Count - 1;
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        IInteractable selectedInter = allInteractables[selectedIndex];

        foreach (var kvp in allItemButtons)
        {
            InteractItemUI itemUI = kvp.Value.GetComponent<InteractItemUI>();
            itemUI.isSelected = kvp.Key == selectedInter;
        }
        
        if(scrollRect != null && allInteractables.Count > 1)
        {
            float normalizedScroll = 1f - ((float)selectedIndex / (allInteractables.Count - 1));
            scrollRect.verticalNormalizedPosition = normalizedScroll;
        }
    }

    public void HandleScrollSelect()
    {
        if (allInteractables.Count <= 1)
            return;

        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
        {
            if (scroll > 0)
                selectedIndex--;
            else
                selectedIndex++;

            if (selectedIndex < 0)
                selectedIndex = allInteractables.Count - 1;
            else if (selectedIndex >= allInteractables.Count)
                selectedIndex = 0;

            UpdateUI();
        }
    }
    
    public IInteractable GetSelectedInteractable()
    {
        if (allInteractables.Count == 0)
            return null;

        return allInteractables[selectedIndex];
    }
}