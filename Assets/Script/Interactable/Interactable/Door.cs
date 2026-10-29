using System.Collections;
using UnityEngine;
using Unity.AI.Navigation;
using System.Collections.Generic;

[RequireComponent(typeof(DoorSound))]
public class Door : Interactable
{
    private static readonly int IsOpenHash = Animator.StringToHash("IsOpen");

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

    public static float LastRouteChangeTime { get; private set; } = float.NegativeInfinity;

    public bool IsOpen { get; private set; }
    public bool IsLocked => currentLock != null && currentLock.IsLocked;
    public Transform PlayerPointFront => playerPointFront;
    public Transform PlayerPointBack => playerPointBack;

    public Door Root { get; private set; }

    private DoorSound sound;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        col = GetComponent<Collider>();
        currentLock = GetComponentInChildren<Lock>();
        Root = FindRootDoor();
        sound = GetComponent<DoorSound>();

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
        {
            sound.PlayLockedSound();
            anim.SetTrigger("Locked");
            return;
        }

        PassThrough(interactor);
    }
    
    private void PassThrough(GameObject interactor)
    {
        Transform targetPoint = GetClosestPoint(interactor.transform.position);

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

    public void RestoreState(bool open, bool locked)
    {
        if (currentLock != null && currentLock.IsLocked != locked)
            currentLock.RestoreLocked(locked);

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

        if (link != null)
            link.enabled = true;

        if (anim != null)
            anim.SetBool(IsOpenHash, IsOpen);

        if (colliderRoutine != null)
            StopCoroutine(colliderRoutine);
        colliderRoutine = StartCoroutine(DisableColliderFor(colDisableWaitTime));

        if (open)
            sound.PlayOpenSound();
        else
            sound.PlayCloseSound();
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