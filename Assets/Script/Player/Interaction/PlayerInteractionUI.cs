using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class PlayerInteractionUI
{
    [SerializeField] private GameObject listPanel;

    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private Transform itemButtonParent;

    private Dictionary<IInteractable, GameObject> allItemButtons = new();

    private PlayerInteractionDetector detector;

    public void Initialize(PlayerInteractionDetector detector)
    {
        this.detector = detector;
    }

    public void UpdateDisplay()
    {
        List<IInteractable> allInteractables = detector.GetAllDetected();
        List<IInteractable> itemsToRemove = new();

        foreach (var kvp in allItemButtons)
        {
            if (!allInteractables.Contains(kvp.Key))
            {
                Object.Destroy(kvp.Value);
                itemsToRemove.Add(kvp.Key);
            }
        }

        foreach (var item in itemsToRemove)
        {
            allItemButtons.Remove(item);
        }
        
        foreach(IInteractable interactable in allInteractables)
        {
            GameObject itemButton = Object.Instantiate(itemButtonPrefab, itemButtonParent);
            allItemButtons.Add(interactable, itemButton);

            TMP_Text nameText = itemButton.GetComponentInChildren<TMP_Text>();
            nameText.text = interactable.Owner.name;
        }
    }
}