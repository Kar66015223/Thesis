using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyVision))]
[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyDoorNavigation))]
public class EnemyController : MonoBehaviour, IHearable, IPlayerCatcher, IStunnable, IBumpable
{
    public NavMeshAgent Agent { get; private set; }
    public EnemyVision Vision { get; private set; }
    public EnemyMovement Movement { get; private set; }
    public EnemyDoorNavigation DoorNavigation { get; private set; }
    public CinemachineImpulseSource ImpulseSource { get; private set; }
    public EnemyStepTracking Tracking { get; } = new();
    public IEnemyState CurrentState { get; private set; }

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

    // IPlayerCatcher
    public float CatchDamage => catchDamage;
    public float DamageInterval => damageCooldown;
    // IPlayerCatcher

    public float StunTime => stunTime;

    // The player hits the enemy every frame they keep pushing; contact with gaps shorter than this is one bump.
    private const float BumpContactGap = 0.25f;
    private const float RoutineSnapRadius = 2f;
    private float lastBumpTime = float.NegativeInfinity;

    public bool IsChasing => CurrentState is ChaseState;
    public bool IsInDefaultState => CurrentState is IdleState || CurrentState is PatrolState;

    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Vision = GetComponent<EnemyVision>();
        Movement = GetComponent<EnemyMovement>();
        DoorNavigation = GetComponent<EnemyDoorNavigation>();
        ImpulseSource = GetComponent<CinemachineImpulseSource>();

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
        if(!handlingDoor)
            CurrentState?.Update();
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

    /// <summary>Route step changed to "stand here".</summary>
    public void ApplyIdlePoint(Transform point)
    {
        idlePoint = point;
        patrolPoints = new Waypoint[0];

        if (idlePoint != null)
            SetDestination(idlePoint.position);
    }

    /// <summary>Route step changed to "patrol these points".</summary>
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
    
    // Senses
    public void OnHearSound(SoundSignal sound) => CurrentState?.OnHearSound(sound);
    public void OnSeenTarget(Transform target) => CurrentState?.OnSeenTarget(target);

    // IStunnable
    public void Stun() => ChangeState(new StunnedState(this));

    // IBumpable: treated as a "look at" noise where the player is, so each state decides like it does for sounds.
    // Patrol, Idle and Distracted switch to DistractedState; Confirming, Chase, Catching and Stunned ignore it
    // instead of being cut short.
    public void OnBumped(Transform bumper)
    {
        bool isNewBump = Time.time - lastBumpTime > BumpContactGap;
        lastBumpTime = Time.time;

        if (isNewBump)
            CurrentState?.OnHearSound(new SoundSignal(bumper.position, 0f, EnemyReaction.LookAt));
    }

    // IPlayerCatcher
    public void OnCatchDamageDealt() => ShakeScreen();

    public void ReleasePlayer(bool playerEscaped)
    {
        if (playerEscaped)
            Stun();
        else
            ReturnToDefaultState();
    }

    // Chasing, searching or being stunned was about the player before they died, so drop it.
    // Teleport rather than walk back, so the enemies are in place the moment the player gets control again.
    private void HandlePlayerRespawned() => SnapToRoutine();

    /// <summary>
    /// Puts the enemy straight onto the spot its routine walks to (the idle point, or the first patrol point,
    /// facing the same way) and restarts the routine from there.
    /// </summary>
    public void SnapToRoutine()
    {
        Transform spot = patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[0] != null
            ? patrolPoints[0].wayPoint
            : idlePoint;

        // A half-finished door approach would otherwise keep steering towards the old door after the warp.
        DoorNavigation.CancelDoorApproach();

        // Points can sit a little above the floor, and Warp needs a spot on the NavMesh.
        if (spot != null && NavMesh.SamplePosition(spot.position, out NavMeshHit hit, RoutineSnapRadius, Agent.areaMask)
            && Agent.Warp(hit.position))
        {
            transform.rotation = Quaternion.Euler(0f, spot.eulerAngles.y, 0f);
        }

        // Restart even an enemy that was already patrolling, so it heads on from the new spot instead of an old target.
        ReturnToDefaultState();
    }

    private void DetachFromSelf(Transform child)
    {
        if (child != null && child.parent == transform)
            child.SetParent(null);
    }
}
