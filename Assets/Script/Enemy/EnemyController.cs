using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [field: SerializeField] public EnemyState CurrentState { get; private set; }

    private NavMeshAgent agent;
    public EnemyPatrol patrol = new();

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        patrol.Initialize(this, agent);
    }

    void Update()
    {
        switch(CurrentState)
        {
            case EnemyState.Patrol:
                patrol.UpdatePatrol();
                break;
        }
    }
}
