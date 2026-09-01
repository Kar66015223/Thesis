using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerInteractionDetector
{
    private List<IInteractable> allDetected = new();
    [SerializeField] private List<GameObject> allDetectedObj = new();

    void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            if (!allDetected.Contains(interactable))
            {
                allDetected.Add(interactable);
                allDetectedObj.Add(interactable.Owner);
                Debug.Log($"Found {other.name}");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (allDetected.Contains(interactable))
        {
            allDetected.Remove(interactable);
            allDetectedObj.Remove(interactable.Owner);
        }
    }

    public List<IInteractable> GetAllDetected() => allDetected;
    public List<GameObject> GetAllDetectObj() => allDetectedObj;
}