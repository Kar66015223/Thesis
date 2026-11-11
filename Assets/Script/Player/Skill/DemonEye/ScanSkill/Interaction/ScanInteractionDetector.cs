using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class ScanInteractionDetector
{
    private Camera cam;
    [SerializeField] private float interactRange = 10f;
    [SerializeField] private LayerMask blockingMask = Physics.DefaultRaycastLayers;

    private IScanInteractable curInteractable;

    public void Initialize()
    {
        cam = Camera.main;
    }

    public void CheckInteractable()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            if (hit.collider.TryGetComponent(out IScanInteractable iScan))
                curInteractable = iScan;
        }
        else
            curInteractable = null;

        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.yellow);
    }
}