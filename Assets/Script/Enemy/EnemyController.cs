using System.Transactions;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyVision))]
public class EnemyController : MonoBehaviour, IHearable
{
    public NavMeshAgent Agent { get; private set; }
    public EnemyVision Vision { get; private set; }
    [SerializeField] private TMP_Text stateUIText;

    private IEnemyState currentState;

    [Header("Shared Settings")]
    public IEnemyState initialState;

    public float walkSpeed = 3f;
    public float runSpeed = 5f;

    public Transform idleStandPoint;
    // public Transform[] idleLookPoints;
    // public float idleLookWaitTime = 1f;

    public Transform[] patrolWaypoints;
    public float patrolWaitTime = 1f;

    public float distractWaitTime = 1f;
    public float lookAtRotationSpeed = 5f;

    public float stunTime = 5f;

    public TMP_Text escapePrompt;

    void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Vision = GetComponent<EnemyVision>();

        if (patrolWaypoints.Length > 0)
        {
            foreach (var dest in patrolWaypoints)
            {
                if (dest != null)
                {
                    if (dest.parent == transform)
                        dest.SetParent(null);
                }
                
                ChangeState(new PatrolState(this));
            }
        }
        else if(idleStandPoint != null)
        {
            if (idleStandPoint.parent == transform)
                idleStandPoint.SetParent(null);

            ChangeState(new IdleState(this));
        }

        initialState = currentState;

        if(initialState != null)
            Debug.Log($"{gameObject.name}'s initial state is {initialState.GetType().Name}");
    }

    void Update()
    {
        currentState?.Update();

        if (stateUIText != null && currentState != null)
            stateUIText.text = currentState.GetType().Name;
    }

    public void ChangeState(IEnemyState newState)
    {
        Debug.Log($"{gameObject.name} change state from {currentState} to {newState}");
        
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    public void OnHearSound(SoundSignal sound)
    {
        currentState?.OnHearSound(sound);
    }

    public void OnSeenTarget(Transform target)
    {
        currentState?.OnSeenTarget(target);
    }
}
