using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimation : MonoBehaviour
{
    [SerializeField] private Animator anim;
    private NavMeshAgent agent;

    public void Initialize(NavMeshAgent agent) => this.agent = agent;
}