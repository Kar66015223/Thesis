using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerInteractionDetector
{
    private List<IInteractable> allDetected = new();
    [SerializeField] private List<GameObject> allDetectedObj = new();

    private PlayerInteractionUI ui;

    public void Initialize(PlayerInteractionUI ui)
    {
        this.ui = ui;
    }

    public void AddDetected(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            if (!interactable.CanInteract())
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
            
        allDetected.RemoveAll(item => 
            item == null || 
            item.Owner == null || 
            !item.CanInteract());

        allDetectedObj.RemoveAll(item => item == null);

        ui.UpdateDisplay();
    }

    public List<IInteractable> GetAllDetected() => allDetected;
    public List<GameObject> GetAllDetectedObj() => allDetectedObj;
}