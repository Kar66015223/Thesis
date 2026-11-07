using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections.Generic;

[RequireComponent(typeof(DoorSound))]
public class Door : Interactable
{
    private static readonly int IsOpenHash = Animator.StringToHash("IsOpen");

    private const float DefaultStationaryTime = 0.25f;
    private const float SettleMargin = 0.15f;
    private const float MovedDistanceSqr = 0.000001f;
    private const float MovedAngle = 0.05f;

    private static readonly List<Door> activeDoors = new();
    public static IReadOnlyList<Door> ActiveDoors => activeDoors;

    [SerializeField] private Transform playerPointFront;
    [SerializeField] private Transform playerPointBack;
    [SerializeField] private float colDisableWaitTime;
    [SerializeField] private NavMeshLink link;
    [Tooltip("Longest an enemy waits for this door to finish opening and re-carve the NavMesh before walking through.")]
    [SerializeField, Min(0f)] private float maxSettleTime = 3f;

    private Animator anim;
    private Collider[] cols;
    private Lock currentLock;
    private Coroutine colliderRoutine;

    private Transform[] movingParts;
    private Vector3[] lastPartPositions;
    private Quaternion[] lastPartRotations;
    private float requiredStationaryTime;
    private float stationaryTimer;
    private float toggleTime;

    public static float LastRouteChangeTime { get; private set; } = float.NegativeInfinity;

    public bool IsOpen { get; private set; }
    public bool IsSettled { get; private set; } = true;
    public bool IsLocked => currentLock != null && currentLock.IsLocked;
    public Transform PlayerPointFront => playerPointFront;
    public Transform PlayerPointBack => playerPointBack;

    public Door Root { get; private set; }

    private DoorSound sound;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        if (anim == null)
            anim = GetComponentInParent<Animator>();
            
        cols = GetComponentsInChildren<Collider>();
        CacheMovingParts();
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

    void LateUpdate()
    {
        if (IsSettled)
            return;

        stationaryTimer = HavePartsMoved() ? 0f : stationaryTimer + Time.deltaTime;
        RecordPartPoses();

        if (stationaryTimer >= requiredStationaryTime + SettleMargin || Time.time - toggleTime >= maxSettleTime)
            IsSettled = true;
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
            link.enabled = open;

        if (anim != null)
            anim.SetBool(IsOpenHash, IsOpen);

        BeginSettling();

        if (colliderRoutine != null)
            StopCoroutine(colliderRoutine);
        colliderRoutine = StartCoroutine(DisableColliderFor(colDisableWaitTime));

        if (open)
            sound.PlayOpenSound();
        else
            sound.PlayCloseSound();
    }
    
    private void CacheMovingParts()
    {
        List<Transform> parts = new();
        requiredStationaryTime = DefaultStationaryTime;

        foreach (NavMeshObstacle obstacle in GetComponentsInChildren<NavMeshObstacle>())
        {
            parts.Add(obstacle.transform);
            if (obstacle.carving && obstacle.carveOnlyStationary)
                requiredStationaryTime = Mathf.Max(requiredStationaryTime, obstacle.carvingTimeToStationary);
        }

        if (parts.Count == 0)
        {
            foreach (Collider col in cols)
            {
                if (!col.isTrigger)
                    parts.Add(col.transform);
            }
        }

        movingParts = parts.ToArray();
        lastPartPositions = new Vector3[movingParts.Length];
        lastPartRotations = new Quaternion[movingParts.Length];
    }

    private void BeginSettling()
    {
        IsSettled = false;
        toggleTime = Time.time;
        stationaryTimer = 0f;
        RecordPartPoses();
    }

    private void RecordPartPoses()
    {
        for (int i = 0; i < movingParts.Length; i++)
        {
            if (movingParts[i] == null)
                continue;

            lastPartPositions[i] = movingParts[i].position;
            lastPartRotations[i] = movingParts[i].rotation;
        }
    }

    private bool HavePartsMoved()
    {
        for (int i = 0; i < movingParts.Length; i++)
        {
            Transform part = movingParts[i];
            if (part == null)
                continue;

            if ((part.position - lastPartPositions[i]).sqrMagnitude > MovedDistanceSqr ||
                Quaternion.Angle(part.rotation, lastPartRotations[i]) > MovedAngle)
                return true;
        }

        return false;
    }

    private IEnumerator DisableColliderFor(float seconds)
    {
        if (cols == null)
            yield break;

        foreach (var col in cols)
            col.enabled = false;

        yield return new WaitForSeconds(seconds);

        foreach(var col in cols)
        {
            col.enabled = true;
            colliderRoutine = null;
        }
    }
}