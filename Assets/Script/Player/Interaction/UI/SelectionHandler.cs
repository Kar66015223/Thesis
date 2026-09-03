using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class SelectionHandler
{
    private PlayerInteractionDetector detector;
    private PlayerInteractionHandler handler;
    private PlayerInteractionUI ui;

    public void Initialize(
        PlayerInteractionDetector detector, PlayerInteractionHandler handler, PlayerInteractionUI ui)
    {
        this.detector = detector;
        this.handler = handler;
        this.ui = ui;
    }

    public void UpdateSelection()
    {
        if (ui.GetAllItemButtons().Count > 0 && detector.GetAllDetected().Count > 0)
        {
            InteractItemUI firstItem = ui.GetAllItemButtons().First().GetComponent<InteractItemUI>();
            IInteractable firstInter = detector.GetAllDetected().First();

            SelectItem(firstItem, firstInter);
        }
    }

    public void SelectItem(InteractItemUI selectedUI, IInteractable selectedInter)
    {
        List<GameObject> allItemButtonsObj = ui.GetAllItemButtonsPair().Values.ToList();
        List<InteractItemUI> allButtonsInteractUI = new();

        foreach (var button in allItemButtonsObj)
        {
            allButtonsInteractUI.Add(button.GetComponent<InteractItemUI>());
        }

        if (allButtonsInteractUI.Count > 0 && allButtonsInteractUI.Contains(selectedUI))
        {
            foreach (var itemUI in allButtonsInteractUI)
            {
                itemUI.isSelected = false;
                selectedUI.isSelected = true;

                handler.SetSelected(selectedInter);
            }
        }
    }
}