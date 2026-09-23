using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerInteractionDetector
{
    private List<IInteractable> allDetected = new();
    [SerializeField] private List<GameObject> allDetectedObj = new();

    private GameObject player;
    private PlayerInteractionUI ui;

    public void Initialize(GameObject player, PlayerInteractionUI ui)
    {
        this.player = player;
        this.ui = ui;
    }

    public void AddDetected(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            if (!interactable.CanInteract(player))
            {
                RemoveDetected(other);
                return;
            }

            if (!allDetected.Contains(interactable))
            {
                allDetected.Add(interactable);
                allDetectedObj.Add(interactable.Owner);

                ui.UpdateDisplay();
            }
        }
    }

    public void RemoveDetected(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            if (allDetected.Contains(interactable))
            {
                allDetected.Remove(interactable);
                allDetectedObj.Remove(interactable.Owner);

                ui.UpdateDisplay();
            }
        }
    }

    public void RemoveInvalids()
    {
        if (allDetected.Count == 0 && allDetectedObj.Count == 0)
            return;
            
        for (int i = allDetected.Count - 1; i >= 0; i--)
        {
            if (allDetected[i] == null || allDetected[i].Owner == null)
            {
                allDetected.RemoveAt(i);
                allDetectedObj.RemoveAt(i);

                Debug.Log($"Removed {allDetected[i].Owner.name}");
            }
        }

        ui.UpdateDisplay();
    }

    public List<IInteractable> GetAllDetected() => allDetected;

    public List<Item> GetAllDetectedItems()
    {
        List<Item> allItems = new();
        foreach (var detected in allDetected)
        {
            if (detected is Item item)
                allItems.Add(item);
        }

        return allItems;
    }

    public List<Interactable> GetAllDetectedInteractables()
    {
        List<Interactable> allInteractable = new();
        foreach (var detected in allDetected)
        {
            if (detected is Interactable interact)
                allInteractable.Add(interact);
        }

        return allInteractable;
    }
    
    public List<GameObject> GetAllDetectedObj() => allDetectedObj;
}