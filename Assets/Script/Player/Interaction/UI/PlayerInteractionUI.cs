using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[System.Serializable]
public class PlayerInteractionUI
{
    [SerializeField] private GameObject listPanel;

    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private Transform itemButtonParent;

    private Dictionary<Item, GameObject> allItemButtonsPair = new();
    [SerializeField] private List<GameObject> allItemButtons = new();

    private List<Interactable> currentActiveInteractables = new();
    [SerializeField] private TMP_Text interactPrompt;

    public SelectionHandler selection = new();
    private GameObject player;
    private PlayerInteractionDetector detector;
    private PlayerInteractionHandler handler;

    public void Initialize(
        GameObject player, PlayerInteractionDetector detector, PlayerInteractionHandler handler)
    {
        this.player = player;
        this.detector = detector;
        this.handler = handler;
        selection.Initialize(allItemButtonsPair);
    }

    public void UpdateDisplay()
    {
        UpdateItemDisplay();
        UpdateInteractableDisplay();
    }

    private void UpdateItemDisplay()
    {
        List<Item> allItems = new();
        foreach (IInteractable interactable in detector.GetAllDetected())
        {
            if (interactable is Item item)
                allItems.Add(item);
        }

        List<Item> itemsToRemove = new();
        List<GameObject> buttonsToRemove = new();

        listPanel.SetActive(allItems.Count > 0);

        foreach (var kvp in allItemButtonsPair)
        {
            if (!allItems.Contains(kvp.Key))
            {
                Object.Destroy(kvp.Value);
                itemsToRemove.Add(kvp.Key);
            }
        }

        foreach (var item in itemsToRemove)
        {
            allItemButtonsPair.Remove(item);
        }

        if (allItemButtons.Count > 0)
            allItemButtons.RemoveAll(obj => obj == null);

        foreach (Item item in allItems)
        {
            if (!allItemButtonsPair.ContainsKey(item))
            {
                GameObject itemButton = Object.Instantiate(itemButtonPrefab, itemButtonParent);
                allItemButtonsPair.Add(item, itemButton);
                allItemButtons.Add(itemButton);

                TMP_Text nameText = itemButton.GetComponentInChildren<TMP_Text>();
                nameText.text = item.Data.itemName;
            }
        }

        selection.UpdateSelection(allItems);
        handler.SetSelectedItem(selection.GetSelectedItem());
    }
    
    private void UpdateInteractableDisplay()
    {
        List<Interactable> allInteractable = detector.GetAllDetected()
        .OfType<Interactable>()
        .ToList();

        foreach (Interactable interact in currentActiveInteractables)
        {
            if (!allInteractable.Contains(interact) && interact != null)
                interact.TogglePrompt(interactPrompt, false, player);
        }

        foreach (Interactable interact in allInteractable)
        {
            bool canInteract = interact.CanInteract(player);
            interact.TogglePrompt(interactPrompt, canInteract, player);
        }

        currentActiveInteractables = allInteractable;

        if (allInteractable.Count > 0)
            handler.SetSelectedInteractable(allInteractable[0]);
        else
            handler.SetSelectedInteractable(null);
    }

    public Dictionary<Item, GameObject> GetAllItemButtonsPair() => allItemButtonsPair;
    public List<GameObject> GetAllItemButtons() => allItemButtons;
}