using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyVision))]
public class EnemyController : MonoBehaviour, IHearable
{
    [field: SerializeField] public EnemyState CurrentState { get; private set; } = EnemyState.Idle;

    private NavMeshAgent agent;
    [SerializeField] private EnemyMovement movement = new();
    private EnemyVision vision;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        vision = GetComponent<EnemyVision>();
        movement.Initialize(this, agent);
    }

    void Update()
    {
        switch(CurrentState)
        {
            case EnemyState.Patrol:
                movement.UpdatePatrol();
                break;

            case EnemyState.Distracted:
                movement.UpdateDistracted();
                break;

            case EnemyState.Chase:
                movement.UpdateChase();
                break;
        }
    }

    public void OnHearSound(SoundSignal sound)
    {
        Debug.Log($"{gameObject.name} heared a sound at {sound.Position}");
        movement.SetSoundHeared(sound);
        ChangeState(EnemyState.Distracted);
    }

    public void ChangeState(EnemyState newState)
    {
        CurrentState = newState;
    }
}
