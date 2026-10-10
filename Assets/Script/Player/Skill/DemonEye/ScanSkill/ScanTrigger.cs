using System.Collections.Generic;
using UnityEngine;

public class ScanTrigger : MonoBehaviour
{
    private List<ScannableObject> allScannedEnemies = new();
    private List<ObjectiveHighlight> allScannedObjtive = new();

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out ScannableObject scannable))
        {
            scannable.ApplyHighlight();
            if (!allScannedEnemies.Contains(scannable))
            {
                allScannedEnemies.Add(scannable);
            }
        }

        if(other.TryGetComponent(out ObjectiveHighlight objtive))
        {
            objtive.ApplyHighlight();
            if (!allScannedObjtive.Contains(objtive))
            {
                allScannedObjtive.Add(objtive);
                Debug.Log($"Scanned: {other.gameObject.name}");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out ScannableObject scannable))
        {
            scannable.RemoveHighlight();
            if (allScannedEnemies.Contains(scannable))
            {
                allScannedEnemies.Remove(scannable);
            }
        }

        if(other.TryGetComponent(out ObjectiveHighlight objtive))
        {
            objtive.RemoveHighlight();
            if (allScannedObjtive.Contains(objtive))
            {
                allScannedObjtive.Remove(objtive);
                Debug.Log($"Removed: {other.gameObject.name}");
            }
        }
    }

    void OnDisable()
    {
        if (allScannedEnemies.Count > 0)
        {
            foreach (var scanned in allScannedEnemies)
            {
                scanned.RemoveHighlight();
            }
        }

        if(allScannedObjtive.Count > 0)
        {
            foreach (var scanned in allScannedObjtive)
                scanned.RemoveHighlight();
        }

        allScannedEnemies.Clear();
        allScannedObjtive.Clear();
    }
}