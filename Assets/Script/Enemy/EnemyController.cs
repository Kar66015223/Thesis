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
    public float walkSpeed = 3f;
    public float runSpeed = 5f;

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

        if (patrolWaypoints != null)
        {
            foreach (var dest in patrolWaypoints)
            {
                if (dest != null && dest.parent == transform)
                    dest.SetParent(null);
            }
        }

        ChangeState(new PatrolState(this));
    }

    void Update()
    {
        currentState?.Update();

        if (stateUIText != null && currentState != null)
            stateUIText.text = currentState.GetType().Name;
    }

    public void ChangeState(IEnemyState newState)
    {
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
