using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class PlayerInteractionDetector
{
    private List<IInteractable> allDetected = new();
    [SerializeField] private List<GameObject> allDetectedObj = new();

    private GameObject player;
    private PlayerInteractionUI ui;
    private Collider detectionCol;

    public void Initialize(GameObject player, PlayerInteractionUI ui, Collider col)
    {
        this.player = player;
        this.ui = ui;
        detectionCol = col;
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
        for (int i = allDetected.Count - 1; i >= 0; i--)
        {
            IInteractable interactable = allDetected[i];

            bool invalid =
                interactable == null || (interactable is Object unityObj && unityObj == null);

            GameObject owner = null;

            if (!invalid)
            {
                owner = interactable.Owner;

                invalid =
                    owner == null || !interactable.CanInteract(player) || !IsOverlapping(owner);
            }

            if (!invalid)
                continue;

            string objName =
                owner != null ? owner.name : "<destroyed interactable>";

            allDetected.RemoveAt(i);

            if (i < allDetectedObj.Count)
                allDetectedObj.RemoveAt(i);

            // Debug.Log($"Removed invalid: {objName}");
        }

        ui.UpdateDisplay();
    }
    
    private bool IsOverlapping(GameObject owner)
    {
        if (detectionCol == null || !detectionCol.enabled || !detectionCol.gameObject.activeInHierarchy)
            return false;

        Collider[] targetCol = owner.GetComponentsInChildren<Collider>();
        foreach (Collider target in targetCol)
        {
            if (target == null ||
                target == detectionCol ||
                !target.enabled ||
                !target.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (!detectionCol.bounds.Intersects(target.bounds))
                continue;

            bool overlapping = Physics.ComputePenetration(
                detectionCol,
                detectionCol.transform.position,
                detectionCol.transform.rotation,
                target,
                target.transform.position,
                target.transform.rotation,
                out _,
                out _
            );

            if (overlapping)
                return true;
        }

        return false;
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