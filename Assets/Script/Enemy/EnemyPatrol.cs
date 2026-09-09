using UnityEngine;
using System.Linq;
using UnityEngine.AI;

[System.Serializable]
public class EnemyPatrol
{
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float waitTime = 1f;

    [SerializeField] private Transform[] dests;
    private int curTargetIndex;
    private float nextMoveTime;

    private NavMeshAgent agent;

    public void Initialize(EnemyController controller, NavMeshAgent agent)
    {
        this.agent = agent;

        dests = controller.GetComponentsInChildren<Transform>()
            .Where(d => d != controller.gameObject.transform).ToArray();

        if (dests.Length != 0)
        {
            foreach (var dest in dests)
            {
                dest.SetParent(null);
            }
        }

        this.agent.speed = patrolSpeed;
    }

    public void UpdatePatrol()
    {
        if (dests == null || dests.Length < 2)
            return;

        if (Time.time < nextMoveTime)
            return;

        Transform target = dests[curTargetIndex];
        agent.SetDestination(target.position);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            nextMoveTime = Time.time + waitTime;
            curTargetIndex = (curTargetIndex + 1) % dests.Length;
        }   
    }
}