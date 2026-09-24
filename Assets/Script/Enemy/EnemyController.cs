using TMPro;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyVision))]
public class EnemyController : MonoBehaviour, IHearable
{
    [field: SerializeField] public EnemyState InitialState { get; private set; } = EnemyState.Idle;
    public EnemyState CurrentState { get; private set; }
    [SerializeField] private TMP_Text stateUIText;

    private NavMeshAgent agent;
    [SerializeField] private EnemyMovement movement = new();
    private EnemyVision vision;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        vision = GetComponent<EnemyVision>();
        movement.Initialize(agent, this, vision);

        CurrentState = InitialState;
    }

    void Update()
    {
        switch (CurrentState)
        {
            case EnemyState.Patrol:
                movement.UpdatePatrol();
                break;

            case EnemyState.Distracted:
                movement.UpdateDistracted();
                break;

            case EnemyState.Chasing:
                movement.UpdateChase();
                break;

            case EnemyState.Catching:
                movement.UpdateCatching();
                break;

            case EnemyState.TargetFreed:
                movement.OnTargetFreed();
                break;
        }

        stateUIText.text = CurrentState.ToString();
    }

    public void OnHearSound(SoundSignal sound)
    {
        Debug.Log($"{gameObject.name} heared a sound at {sound.Position}");
        movement.SetSoundHeared(sound);
        ChangeState(EnemyState.Distracted);
    }

    public void OnSeenTarget(Transform target)
    {
        Debug.Log($"{gameObject.name} seen {target.gameObject.name} at {target.position}");
        movement.SetChaseTarget(target);
        ChangeState(EnemyState.Chasing);
    }

    public void ChangeState(EnemyState newState)
        => CurrentState = newState;

    public void ChangeInitialState(EnemyState newState)
        => InitialState = newState;
}
