using UnityEngine;

[System.Serializable]
public class ScanInteractionDetector
{
    private const int HitBufferSize = 16;

    [SerializeField] private float interactRange = 10f;
    [SerializeField] private LayerMask blockingMask = Physics.DefaultRaycastLayers;

    private readonly RaycastHit[] hitBuffer = new RaycastHit[HitBufferSize];

    private Camera cam;
    private Transform owner;
    private Collider curCollider;

    public IScanInteractable Current { get; private set; }
    public bool HasTarget => Current != null && curCollider != null;
    public Vector3 TargetCenter => curCollider.bounds.center;
    public Camera Camera => cam;

    public void Initialize(Transform owner)
    {
        this.owner = owner;
        cam = Camera.main;
    }

    public void CheckInteractable()
    {
        if (cam == null)
            cam = Camera.main;

        if (cam == null)
        {
            Clear();
            return;
        }

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        int count = Physics.RaycastNonAlloc(ray, hitBuffer, interactRange, blockingMask, QueryTriggerInteraction.Ignore);

        Collider nearest = null;
        float nearestDistance = float.PositiveInfinity;

        for (int i = 0; i < count; i++)
        {
            RaycastHit hit = hitBuffer[i];
            if (hit.distance >= nearestDistance || IsOwner(hit.collider))
                continue;

            nearest = hit.collider;
            nearestDistance = hit.distance;
        }

        IScanInteractable found = nearest != null ? nearest.GetComponentInParent<IScanInteractable>() : null;
        if (found == null)
        {
            Clear();
            return;
        }

        Current = found;
        curCollider = nearest;
    }

    public void Clear()
    {
        Current = null;
        curCollider = null;
    }

    private bool IsOwner(Collider col) => owner != null && col.transform.IsChildOf(owner);
}
