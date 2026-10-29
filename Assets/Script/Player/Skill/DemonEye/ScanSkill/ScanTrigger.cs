using System.Collections.Generic;
using UnityEngine;

public class ScanTrigger : MonoBehaviour
{
    private static readonly List<IScannable> componentBuffer = new();
    private readonly HashSet<IScannable> scanned = new();

    void OnTriggerEnter(Collider other)
    {
        other.GetComponents(componentBuffer);
        foreach (IScannable scannable in componentBuffer)
        {
            if (scanned.Add(scannable))
                scannable.OnScanEnter();
        }
    }

    void OnTriggerExit(Collider other)
    {
        other.GetComponents(componentBuffer);
        foreach (IScannable scannable in componentBuffer)
        {
            if (scanned.Remove(scannable))
                scannable.OnScanExit();
        }
    }

    void OnDisable()
    {
        foreach (IScannable scannable in scanned)
        {
            if (scannable is Object unityObj && unityObj != null)
                scannable.OnScanExit();
        }

        scanned.Clear();
    }
}
