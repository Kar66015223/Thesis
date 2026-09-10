using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour, IHearable
{
    [field: SerializeField] public EnemyState CurrentState { get; private set; }

    private NavMeshAgent agent;
    public EnemyMovement movement = new();

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

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
        }
    }

    public void OnHearSound(SoundSignal sound)
    {
        Debug.Log($"{gameObject.name} heared a sound at {sound.Position}");
        movement.SetSoundHeared(sound);
        SwitchState(EnemyState.Distracted);
    }

    public void SwitchState(EnemyState newState)
    {
        CurrentState = newState;
    }
}
