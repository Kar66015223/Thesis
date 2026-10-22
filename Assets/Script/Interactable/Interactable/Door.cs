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

    /// <summary>
    /// Time.time when a door last opened, closed, appeared or disappeared, i.e. when enemy routes may have changed
    /// (see <see cref="DoorRoutePlanner"/>).
    /// </summary>
    public static float LastRouteChangeTime { get; private set; } = float.NegativeInfinity;

    public bool IsOpen { get; private set; }
    public bool IsLocked => currentLock != null && currentLock.IsLocked;
    public Transform PlayerPointFront => playerPointFront;
    public Transform PlayerPointBack => playerPointBack;

    /// <summary>
    /// The door enemies treat as one unit: the topmost Door above this one (a double door's root, whose
    /// Animator swings both leaves), or this door itself.
    /// </summary>
    public Door Root { get; private set; }

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        col = GetComponent<Collider>();
        currentLock = GetComponentInChildren<Lock>();
        Root = FindRootDoor();

        // Closed doors block enemy paths until an enemy opens one (see EnemyDoorNavigation).
        if (link != null)
            link.enabled = IsOpen;
    }

    void Start()
    {
        if (CheckpointManager.Instance != null)
            CheckpointManager.Instance.RegisterDoor(this);
    }

    void OnEnable()
    {
        activeDoors.Add(this);
        LastRouteChangeTime = Time.time;
    }

    void OnDisable()
    {
        activeDoors.Remove(this);
        LastRouteChangeTime = Time.time;
    }

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

    public bool OpenForEnemy()
    {
        if (IsOpen)
            return true;

        if (currentLock != null && currentLock.IsLocked)
            currentLock.ForceUnlock();

        SetDoorOpen(true);
        return true;
    }

    /// <summary>Puts the door back to how it was at a checkpoint.</summary>
    public void RestoreState(bool open, bool locked)
    {
        if (currentLock != null && currentLock.IsLocked != locked)
            currentLock.RestoreLocked(locked);

        // Only when it changed: SetDoorOpen also turns the enemy NavMeshLink on for good.
        if (IsOpen != open)
            SetDoorOpen(open);
    }

    private Door FindRootDoor()
    {
        Door root = this;
        for (Transform parent = transform.parent; parent != null; parent = parent.parent)
        {
            if (parent.TryGetComponent(out Door door))
                root = door;
        }

        return root;
    }

    private void SetDoorOpen(bool open)
    {
        IsOpen = open;
        LastRouteChangeTime = Time.time;

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