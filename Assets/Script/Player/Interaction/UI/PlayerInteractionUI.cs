using System.Collections.Generic;
using TMPro;
using Unity.Multiplayer.Center.Common;
using UnityEngine;

[System.Serializable]
public class PlayerInteractionUI
{
    [SerializeField] private GameObject listPanel;

    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private Transform itemButtonParent;

    private Dictionary<IInteractable, GameObject> allItemButtonsPair = new();
    [SerializeField] private List<GameObject> allItemButtons = new();

    public SelectionHandler selection = new();
    private PlayerInteractionDetector detector;
    private PlayerInteractionHandler handler;

    public void Initialize(PlayerInteractionDetector detector, PlayerInteractionHandler handler)
    {
        this.detector = detector;
        this.handler = handler;
        selection.Initialize(allItemButtonsPair);
    }

    public void UpdateDisplay()
    {
        List<IInteractable> allInteractables = detector.GetAllDetected();
        List<IInteractable> itemsToRemove = new();
        List<GameObject> buttonsToRemove = new();

        listPanel.SetActive(allInteractables.Count > 0);

        foreach (var kvp in allItemButtonsPair)
        {
            if (!allInteractables.Contains(kvp.Key))
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

        foreach (IInteractable interactable in allInteractables)
        {
            if (!allItemButtonsPair.ContainsKey(interactable))
            {
                GameObject itemButton = Object.Instantiate(itemButtonPrefab, itemButtonParent);
                allItemButtonsPair.Add(interactable, itemButton);
                allItemButtons.Add(itemButton);

                TMP_Text nameText = itemButton.GetComponentInChildren<TMP_Text>();
                nameText.text = interactable.Owner.name;
            }
        }

        selection.UpdateSelection(allInteractables);
        handler.SetSelected(selection.GetSelectedInteractable());
    }

    public Dictionary<IInteractable, GameObject> GetAllItemButtonsPair() => allItemButtonsPair;
    public List<GameObject> GetAllItemButtons() => allItemButtons;
}