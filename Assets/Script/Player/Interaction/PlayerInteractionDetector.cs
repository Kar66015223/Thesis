using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks which interactables overlap the player's interaction trigger and can be seen from the player
/// (a line to them hits nothing but the target or the player). Nothing else: whether they can be used and
/// which one is selected is decided by <see cref="PlayerInteractionHandler"/>.
/// </summary>
public class PlayerInteractionDetector
{
    private static readonly List<Collider> colliderBuffer = new();
    private static readonly IComparer<RaycastHit> NearestFirst =
        Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance));

    private readonly List<IInteractable> detected = new();
    private RaycastHit[] hitBuffer = new RaycastHit[8];

    // A blocked target is re-checked every frame, so only log when its list of blockers changes.
    private readonly Dictionary<GameObject, string> loggedBlockers = new();
    private readonly List<string> blockerNames = new();
    private Collider detectionCol;
    private CharacterController body;
    private LayerMask blockingMask;

    public IReadOnlyList<IInteractable> Detected => detected;

    private Transform rayStart;

    /// <param name="body">The player: rays start at the top of its capsule and its own colliders never block.</param>
    public void Initialize(Collider col, CharacterController body, LayerMask blockingMask, Transform rayStart)
    {
        detectionCol = col;
        this.body = body;
        this.blockingMask = blockingMask;
        this.rayStart = rayStart;
    }

    public void Add(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable) && !detected.Contains(interactable)
            && HasLineOfSight(interactable.Owner))
        {
            detected.Add(interactable);
        }
    }

    public void Remove(Collider other)
    {
        if (other.TryGetComponent(out IInteractable interactable))
        {
            detected.Remove(interactable);

            // Walking away and back logs the blockers again.
            if (interactable.Owner != null)
                loggedBlockers.Remove(interactable.Owner);
        }
    }

    /// <summary>
    /// OnTriggerExit is not called when a collider is disabled or destroyed, so prune those manually.
    /// Also drops targets something has moved in front of, using the same test as <see cref="Add"/> so they don't flicker.
    /// </summary>
    public void RemoveInvalids()
    {
        for (int i = detected.Count - 1; i >= 0; i--)
        {
            IInteractable interactable = detected[i];
            bool isDestroyed = interactable == null || (interactable is Object unityObj && unityObj == null);

            if (isDestroyed || interactable.Owner == null || !IsOverlapping(interactable.Owner)
                || !HasLineOfSight(interactable.Owner))
            {
                detected.RemoveAt(i);
            }
        }
    }

    private bool HasLineOfSight(GameObject owner)
    {
        if (body == null || owner == null)
            return true;

        // From the top of the capsule (about eye level), not its centre, so items on a table aren't hidden behind its edge.
        // TransformPoint instead of bounds: the controller is turned off while hiding, and a disabled collider has no bounds.
        Vector3 eyeOffset = Vector3.up * Mathf.Max(0f, body.height / 2f - body.radius);
        Vector3 from = rayStart.position;
        Vector3 to = GetTargetPoint(owner);
        Vector3 toTarget = to - from;
        float distance = toTarget.magnitude;
        if (distance <= Mathf.Epsilon)
            return true;

        // NonAlloc drops hits that don't fit, which could hide a blocker: grow and cast again.
        int count;
        while ((count = Physics.RaycastNonAlloc(from, toTarget / distance, hitBuffer, distance, blockingMask,
                   QueryTriggerInteraction.Ignore)) == hitBuffer.Length)
        {
            hitBuffer = new RaycastHit[hitBuffer.Length * 2];
        }

        // Hits come back unordered; walk them nearest first so the ray can stop where it reaches the target.
        System.Array.Sort(hitBuffer, 0, count, NearestFirst);

        blockerNames.Clear();
        Collider firstBlocker = null;
        Vector3 rayEnd = to;
        for (int i = 0; i < count; i++)
        {
            Collider hit = hitBuffer[i].collider;
            if (hit.transform.IsChildOf(body.transform))
                continue;

            // Reached the target's surface: whatever lies further along (inside or behind it) doesn't count.
            if (hit.transform.IsChildOf(owner.transform))
            {
                rayEnd = hitBuffer[i].point;
                break;
            }

            if (firstBlocker == null)
                firstBlocker = hit;

            blockerNames.Add($"{hit.name} (layer: {LayerMask.LayerToName(hit.gameObject.layer)})");
        }

        bool blocked = firstBlocker != null;
        if (blocked)
            LogBlockers(owner, firstBlocker);
        else
            loggedBlockers.Remove(owner);

#if UNITY_EDITOR
        // Scene view only: green reached the target (and stops on its surface), red is blocked by something in between.
        Debug.DrawLine(from, rayEnd, blocked ? Color.red : Color.green);
#endif
        return !blocked;
    }

    private void LogBlockers(GameObject owner, Collider firstBlocker)
    {
        string report = string.Join(", ", blockerNames);
        if (loggedBlockers.TryGetValue(owner, out string lastReport) && lastReport == report)
            return;

        loggedBlockers[owner] = report;

        // Clicking the log selects the nearest blocker in the Hierarchy.
        // Debug.Log($"Can't interact with {owner.name}, blocked by: {report}", firstBlocker);
    }

    /// <summary>Centre of the target's solid colliders (its trigger zones only if it has nothing else).</summary>
    private static Vector3 GetTargetPoint(GameObject owner)
    {
        owner.GetComponentsInChildren(false, colliderBuffer);

        if (TryGetColliderBounds(solidOnly: true, out Bounds bounds) || TryGetColliderBounds(solidOnly: false, out bounds))
            return bounds.center;

        return owner.transform.position;
    }

    private static bool TryGetColliderBounds(bool solidOnly, out Bounds bounds)
    {
        bounds = default;
        bool found = false;

        foreach (Collider col in colliderBuffer)
        {
            if (!col.enabled || (solidOnly && col.isTrigger))
                continue;

            if (found)
                bounds.Encapsulate(col.bounds);
            else
                bounds = col.bounds;

            found = true;
        }

        return found;
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
