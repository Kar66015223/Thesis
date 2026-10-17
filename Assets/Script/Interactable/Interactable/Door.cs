using System.Collections;
using UnityEngine.AI;
using UnityEngine;
using Unity.AI.Navigation;
using System.Collections.Generic;

/// <summary>
/// A door the player walks through and enemies can open. Locking rules are delegated to an optional
/// <see cref="Lock"/> child (Strategy pattern), so new lock types never require changing this class.
/// </summary>
public class Door : Interactable
{
    private static readonly int IsOpenHash = Animator.StringToHash("IsOpen");

    // Registry so enemies don't have to call FindObjectsByType<Door>() every time they're blocked.
    private static readonly List<Door> activeDoors = new();
    public static IReadOnlyList<Door> ActiveDoors => activeDoors;

    [SerializeField] private Transform playerPointFront;
    [SerializeField] private Transform playerPointBack;
    [SerializeField] private float colDisableWaitTime;
    [SerializeField] private NavMeshLink link;

    private Animator anim;
    private Collider col;
    private Lock currentLock;
    private Coroutine colliderRoutine;

    public bool IsOpen { get; private set; }

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        col = GetComponent<Collider>();
        currentLock = GetComponentInChildren<Lock>();

        // Closed doors block enemy paths until an enemy opens one (see EnemyDoorNavigation).
        if (link != null)
            link.enabled = IsOpen;
    }

    void OnEnable() => activeDoors.Add(this);
    void OnDisable() => activeDoors.Remove(this);

    protected override void OnInteract(GameObject interactor)
    {
        if (currentLock != null && currentLock.IsLocked && !currentLock.TryUnlock(interactor))
            return;

        PassThrough(interactor);
    }
    
    private void PassThrough(GameObject interactor)
    {
        Transform targetPoint = GetClosestPoint(interactor.transform.position);

        // Movement is locked by the interact animation (InteractStateBehavior), not here:
        // locking it here froze the player after a password door, which plays no animation.
        if (interactor.TryGetComponent(out IInteractorBody body))
            body.Teleport(targetPoint.position, targetPoint.rotation);
        else
            interactor.transform.position = targetPoint.position;

        SetDoorOpen(!IsOpen);
    }

    public Transform GetClosestPoint(Vector3 position)
    {
        float distanceToFront = (position - playerPointFront.position).sqrMagnitude;
        float distanceToBack = (position - playerPointBack.position).sqrMagnitude;

        return distanceToFront < distanceToBack ? playerPointFront : playerPointBack;
    }

    public bool TryGetReachableApproach(Vector3 from, int areaMask, out Transform approachPoint)
    {
        approachPoint = null;

        bool canReachFront = TryGetPathLength(from, playerPointFront, areaMask, out float frontLength);
        bool canReachBack = TryGetPathLength(from, playerPointBack, areaMask, out float backLength);

        if (!canReachFront && !canReachBack)
            return false;

        approachPoint = canReachFront && (!canReachBack || frontLength <= backLength)
            ? playerPointFront : playerPointBack;

        return true;
    }

    public bool OpenForEnemy()
    {
        if (IsOpen)
            return true;

        if (currentLock != null && currentLock.IsLocked)
            currentLock.ForceUnlock();

        SetDoorOpen(true);
        return true;
    }

    private static bool TryGetPathLength(Vector3 from, Transform point, int areaMask, out float pathLength)
    {
        pathLength = 0f;

        if (point == null)
            return false;

        NavMeshPath path = new();
        bool calculated = NavMesh.CalculatePath(from, point.position, areaMask, path);

        if (!calculated || path.status != NavMeshPathStatus.PathComplete)
            return false;

        Vector3[] corners = path.corners;
        for (int i = 1; i < corners.Length; i++)
            pathLength += Vector3.Distance(corners[i - 1], corners[i]);

        return true;
    }

    private void SetDoorOpen(bool open)
    {
        IsOpen = open;

        // Once a door has been opened the link stays on, so an agent that is already
        // crossing it never has the link disabled under its feet.
        if (link != null)
            link.enabled = true;

        if (anim != null)
            anim.SetBool(IsOpenHash, IsOpen);

        if (colliderRoutine != null)
            StopCoroutine(colliderRoutine);
        colliderRoutine = StartCoroutine(DisableColliderFor(colDisableWaitTime));
    }
    
    private IEnumerator DisableColliderFor(float seconds)
    {
        if (col == null)
            yield break;

        col.enabled = false;
        yield return new WaitForSeconds(seconds);
        col.enabled = true;
        colliderRoutine = null;
    }
}