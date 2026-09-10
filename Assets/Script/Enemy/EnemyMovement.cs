using UnityEngine;
using System.Linq;
using UnityEngine.AI;

[System.Serializable]
public class EnemyMovement
{
    [Header("Patrol")]
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float waitTime = 1f;

    [SerializeField] private Transform[] dests;
    private int curTargetIndex;
    private float nextMoveTime;

    [Header("Distraction")]
    [SerializeField] private float distractWaitTime = 1f;
    private float distractTimer;
    private bool isDistracted = false;

    public SoundSignal SoundHeared { get; private set; } = null;

    private EnemyController controller;
    private NavMeshAgent agent;

    public void Initialize(EnemyController controller, NavMeshAgent agent)
    {
        this.controller = controller;
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

        if (isDistracted)
            return;

        Transform target = dests[curTargetIndex];
        agent.SetDestination(target.position);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            nextMoveTime = Time.time + waitTime;
            curTargetIndex = (curTargetIndex + 1) % dests.Length;
        }
    }

    public void UpdateDistracted()
    {
        isDistracted = true;
        agent.SetDestination(SoundHeared.Position);

        Debug.Log("Moving to sound");

        if (isDistracted && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Debug.Log("Arrived to sound");
            distractTimer += Time.time;

            if(distractTimer >= distractWaitTime)
            {
                isDistracted = false;
                SoundHeared = null;
                Debug.Log("Finished distracting");

                distractTimer = 0f;

                controller.SwitchState(EnemyState.Patrol);
            }
        }
    }

    public void SetSoundHeared(SoundSignal signal) => SoundHeared = signal; 
}