using System.Collections.Generic;
using UnityEngine;

public class ScanTrigger : MonoBehaviour
{
    private List<ScannableObject> allScanned = new();

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out ScannableObject scannable))
        {
            scannable.ApplyHighlight();
            if (!allScanned.Contains(scannable))
            {
                allScanned.Add(scannable);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out ScannableObject scannable))
        {
            scannable.RemoveHighlight();
            if (allScanned.Contains(scannable))
            {
                allScanned.Remove(scannable);
            }
        }
    }

    void OnDisable()
    {
        if(allScanned.Count > 0)
        {
            foreach(var scanned in allScanned)
            {
                scanned.RemoveHighlight();
            }
        }
        
        allScanned.Clear();
    }
}