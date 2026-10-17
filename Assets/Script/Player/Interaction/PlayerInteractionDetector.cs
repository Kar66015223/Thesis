using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks which interactables physically overlap the player's interaction trigger. Nothing else:
/// whether they can be used and which one is selected is decided by <see cref="PlayerInteractionHandler"/>.
/// </summary>
public class PlayerInteractionDetector
{
    private static readonly List<Collider> colliderBuffer = new();

    private readonly List<IInteractable> detected = new();
    private Collider detectionCol;

    public IReadOnlyList<IInteractable> Detected => detected;

    public void Initialize(Collider col) => detectionCol = col;

    public void Add(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable) && !detected.Contains(interactable))
            detected.Add(interactable);
    }

    public void Remove(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
            detected.Remove(interactable);
    }

    /// <summary>
    /// OnTriggerExit is not called when a collider is disabled or destroyed, so prune those manually.
    /// </summary>
    public void RemoveInvalids()
    {
        for (int i = detected.Count - 1; i >= 0; i--)
        {
            IInteractable interactable = detected[i];
            bool isDestroyed = interactable == null || (interactable is Object unityObj && unityObj == null);

            if (isDestroyed || interactable.Owner == null || !IsOverlapping(interactable.Owner))
                detected.RemoveAt(i);
        }
    }
    
    private bool IsOverlapping(GameObject owner)
    {
        if (detectionCol == null || !detectionCol.enabled || !detectionCol.gameObject.activeInHierarchy)
            return false;

        owner.GetComponentsInChildren(false, colliderBuffer);
        foreach (Collider target in colliderBuffer)
        {
            if (target == detectionCol || !target.enabled)
                continue;

            if (!detectionCol.bounds.Intersects(target.bounds))
                continue;

            bool overlapping = Physics.ComputePenetration(
                detectionCol, detectionCol.transform.position, detectionCol.transform.rotation,
                target, target.transform.position, target.transform.rotation,
                out _, out _);

            if (overlapping)
                return true;
        }

        return false;
    }
}