using TMPro;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyVision))]
public class EnemyController : MonoBehaviour, IHearable
{
    public NavMeshAgent Agent { get; private set; }
    public EnemyVision Vision { get; private set; }
    public IEnemyState CurrentState { get; private set; }

    

    [SerializeField] private TMP_Text stateUIText;

    [Header("Shared Settings")]
    public IEnemyState initialState;

    public float walkSpeed = 3f;
    public float runSpeed = 5f;

    public Transform idlePointsParent;
    public Transform idlePoint;

    public Transform patrolPointsParent;
    public Transform[] patrolPoints;
    public float patrolWaitTime = 1f;

    public float distractWaitTime = 1f;
    public float lookAtRotationSpeed = 5f;

    public float confirmWaitTime = 0.5f;

    public float stunTime = 5f;

    public TMP_Text escapePrompt;

    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Vision = GetComponent<EnemyVision>();

        SetInitialState();
    }

    void Update()
    {
        CurrentState?.Update();

        if (stateUIText != null && CurrentState != null)
            stateUIText.text = CurrentState.GetType().Name;
    }

    private void SetInitialState()
    {
        if (idlePointsParent.parent == transform)
            idlePointsParent.SetParent(null);
        if(patrolPointsParent.parent == transform)
            patrolPointsParent.SetParent(null);

        if (patrolPoints.Length > 0)
        {
            ChangeState(new PatrolState(this));
        }
        else if (idlePoint != null)
        {
            ChangeState(new IdleState(this));
        }

        initialState = CurrentState;

        if (initialState != null)
            Debug.Log($"{gameObject.name}'s initial state is {initialState.GetType().Name}");
    }

    public void ChangeState(IEnemyState newState)
    {
        Debug.Log($"{gameObject.name} change state from {CurrentState} to {newState}");

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState?.Enter();
    }

    public void OnHearSound(SoundSignal sound)
    {
        CurrentState?.OnHearSound(sound);
    }

    public void OnSeenTarget(Transform target)
    {
        CurrentState?.OnSeenTarget(target);
    }

    public void SetIdlePoint(Transform point)
    {
        idlePoint = point;
        patrolPoints = new Transform[0];

        // SetInitialState();
        ChangeState(new IdleState(this));
        
    }
    public void SetPatrolPoints(Transform[] points)
    {
        patrolPoints = points;
        idlePoint = null;

        // SetInitialState();
        ChangeState(new PatrolState(this));
    }
}
