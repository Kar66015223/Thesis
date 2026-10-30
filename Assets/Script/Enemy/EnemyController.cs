using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyVision))]
[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyDoorNavigation))]

[RequireComponent(typeof(EnemyAnimation))]
public class EnemyController : MonoBehaviour, IHearable, IPlayerCatcher, IStunnable, IBumpable
{
    public NavMeshAgent Agent { get; private set; }
    public EnemyVision Vision { get; private set; }
    public EnemyMovement Movement { get; private set; }
    public EnemyDoorNavigation DoorNavigation { get; private set; }
    public CinemachineImpulseSource ImpulseSource { get; private set; }
    public EnemyStepTracking Tracking { get; } = new();
    public IEnemyState CurrentState { get; private set; }

    public EnemyAnimation Anim { get; private set; }

    public event System.Action FoundAnimationFinished;
    public void NotifyFoundAnimationFinished() => FoundAnimationFinished?.Invoke();

    public event System.Action StartCatchAnimationFinished;
    public void NotifyStartCatchAnimationFinished() => StartCatchAnimationFinished?.Invoke();

    [Header("Shared Settings")]
    [SerializeField] private Transform stateUIPoint;
    
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 5f;

    [SerializeField] private Transform idlePointsParent;
    [SerializeField] private Transform idlePoint;

    [SerializeField] private Transform patrolPointsParent;
    [SerializeField] private Waypoint[] patrolPoints;
    [SerializeField] private float patrolWaitTime = 1f;

    [SerializeField] private float distractWaitTime = 1f;
    [SerializeField] private float lookAtRotationSpeed = 5f;

    [SerializeField] private float confirmWaitTime = 0.5f;
    [SerializeField] private float confirmGiveUpTime = 2f;

    [SerializeField] private float catchDamage = 10f;
    [SerializeField] private float damageCooldown = 1f;

    [SerializeField] private float stunTime = 5f;

    [Tooltip("Bone the caught player's neck is held at. Found by name (hand.r) in the model when left empty.")]
    [SerializeField] private Transform rightHand;

    public Transform StateUIPoint => stateUIPoint != null ? stateUIPoint : transform;

    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;

    public Transform IdlePoint => idlePoint;
    
    public IReadOnlyList<Waypoint> PatrolPoints => patrolPoints;
    public float PatrolWaitTime => patrolWaitTime;

    public float DistractWaitTime => distractWaitTime;
    public float LookAtRotationSpeed => lookAtRotationSpeed;

    public float ConfirmWaitTime => confirmWaitTime;
    public float ConfirmGiveUpTime => confirmGiveUpTime;

    public float CatchDamage => catchDamage;
    public float DamageInterval => damageCooldown;

    public float StunTime => stunTime;

    private const float BumpContactGap = 0.25f;
    private const float RoutineSnapRadius = 2f;
    private const string RightHandBoneName = "hand.r";

    public Transform RightHand => rightHand;
    private float lastBumpTime = float.NegativeInfinity;

    public bool IsChasing => CurrentState is ChaseState;
    public bool IsConfirming => CurrentState is ConfirmingState;
    public bool IsInDefaultState => CurrentState is IdleState || CurrentState is PatrolState;

    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Vision = GetComponent<EnemyVision>();
        Movement = GetComponent<EnemyMovement>();
        DoorNavigation = GetComponent<EnemyDoorNavigation>();
        ImpulseSource = GetComponent<CinemachineImpulseSource>();
        Anim = GetComponent<EnemyAnimation>();

        if (rightHand == null)
            rightHand = FindDescendant(transform, RightHandBoneName);
        if (rightHand == null)
            Debug.LogWarning($"{name}: no '{RightHandBoneName}' bone found, assign Right Hand so caught players are held in it.", this);

        DoorNavigation.Initialize(Agent);
        Tracking.Initialize(this);
    }

    void OnEnable() => PlayerEvents.Respawned += HandlePlayerRespawned;
    void OnDisable() => PlayerEvents.Respawned -= HandlePlayerRespawned;

    void Start()
    {
        DetachFromSelf(idlePointsParent);
        DetachFromSelf(patrolPointsParent);

        ReturnToDefaultState();
    }

    void Update()
    {
        bool handlingDoor = DoorNavigation.TickBeforeStateUpdate();
        if (!handlingDoor)
            CurrentState?.Update();

        if (Agent.velocity.magnitude > 1)
            Anim.SetMove(Agent.speed > walkSpeed ? 2 : 1);
        else
            Anim.SetMove(0);
    }

    public void ChangeState(IEnemyState newState)
    {
        DoorNavigation.CancelDoorApproach();

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState?.Enter();

        EnemyEvents.RaiseIndicatorChanged(transform, StateUIPoint, CurrentState?.Indicator ?? EnemyState.None);
    }

    public void ReturnToDefaultState()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
            ChangeState(new PatrolState(this));
        else if (idlePoint != null)
            ChangeState(new IdleState(this));
        else
            Debug.LogWarning($"{name} has no patrol/idle points", this);
    }

    public void SetDestination(Vector3 dest) => DoorNavigation.SetDestination(dest);

    public void ApplyIdlePoint(Transform point)
    {
        idlePoint = point;
        patrolPoints = new Waypoint[0];

        if (idlePoint != null)
            SetDestination(idlePoint.position);
    }

    public void ApplyPatrolPoints(Waypoint[] points)
    {
        patrolPoints = points;
        idlePoint = null;
    }

    public void ShakeScreen()
    {
        if(ImpulseSource != null)
            ImpulseSource.GenerateImpulse();
    }
    
    public void OnHearSound(SoundSignal sound) => CurrentState?.OnHearSound(sound);
    public void OnSeenTarget(Transform target) => CurrentState?.OnSeenTarget(target);

    public void Stun() => ChangeState(new StunnedState(this));

    public void OnBumped(Transform bumper)
    {
        bool isNewBump = Time.time - lastBumpTime > BumpContactGap;
        lastBumpTime = Time.time;

        if (isNewBump)
            CurrentState?.OnHearSound(new SoundSignal(bumper.position, 0f, EnemyReaction.LookAt));
    }

    public void OnCatchDamageDealt() => ShakeScreen();

    public void ReleasePlayer(bool playerEscaped)
    {
        if (playerEscaped)
            Stun();
        else
            ReturnToDefaultState();
    }

    private static Transform FindDescendant(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform found = FindDescendant(child, childName);
            if (found != null)
                return found;
        }

        return null;
    }

    private void HandlePlayerRespawned() => SnapToRoutine();

    public void SnapToRoutine()
    {
        Transform spot = patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[0] != null
            ? patrolPoints[0].wayPoint
            : idlePoint;

        DoorNavigation.CancelDoorApproach();

        if (spot != null && NavMesh.SamplePosition(spot.position, out NavMeshHit hit, RoutineSnapRadius, Agent.areaMask)
            && Agent.Warp(hit.position))
        {
            transform.rotation = Quaternion.Euler(0f, spot.eulerAngles.y, 0f);
        }

        ReturnToDefaultState();
    }

    private void DetachFromSelf(Transform child)
    {
        if (child != null && child.parent == transform)
            child.SetParent(null);
    }
}
