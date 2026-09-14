using UnityEngine;
using System.Linq;
using UnityEngine.AI;

[System.Serializable]
public class EnemyMovement
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 5f;

    [Header("Patrol")]
    [SerializeField] private Transform[] dests;
    [SerializeField] private float patrolWaitTime = 1f;
    private int curTargetIndex;
    private float nextMoveTime;

    [Header("Distraction")]
    [SerializeField] private float lookAtRotationSpeed = 5f;
    
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

        // dests = controller.GetComponentsInChildren<Transform>()
        //     .Where(d => d != controller.gameObject.transform).ToArray();

        if (dests.Length != 0)
        {
            foreach (var dest in dests)
            {
                if(dest.parent == controller.transform)
                    dest.SetParent(null);
            }
        }

        this.agent.speed = walkSpeed;
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
            nextMoveTime = Time.time + patrolWaitTime;
            curTargetIndex = (curTargetIndex + 1) % dests.Length;
        }
    }

    public void UpdateDistracted()
    {
        isDistracted = true;

        switch(SoundHeared.Reaction)
        {
            case EnemyReaction.LookAt:
                Debug.Log($"{controller.gameObject.name} heared LookAt sound, looking at sound");

                agent.updateRotation = false;
                agent.isStopped = true;

                Vector3 direction = SoundHeared.Position - controller.transform.position;
                direction.y = 0;
                
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);

                    controller.transform.rotation = Quaternion.Slerp(
                        controller.transform.rotation,
                        targetRotation,
                        lookAtRotationSpeed * Time.deltaTime);

                    float angleToTarget = Vector3.Angle(controller.transform.forward, direction);
    
                    if(angleToTarget < 5f)
                        distractTimer += Time.deltaTime;
                }

                if(isDistracted && distractTimer >= distractWaitTime)
                {
                    agent.updateRotation = true;
                    agent.isStopped = false;

                    isDistracted = false;
                    SoundHeared = null;
                    distractTimer = 0f;
                    controller.SwitchState(EnemyState.Patrol);

                    Debug.Log("Finished checking LookAt sound");
                }
                break;
                
            case EnemyReaction.WalkTo:
                Debug.Log($"{controller.gameObject.name} heared WalkTo sound, walking to sound");

                agent.SetDestination(SoundHeared.Position);

                if (isDistracted && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    Debug.Log("Arrived at sound");
                    distractTimer += Time.deltaTime;

                    if (distractTimer >= distractWaitTime)
                    {
                        isDistracted = false;
                        SoundHeared = null;
                        distractTimer = 0f;
                        controller.SwitchState(EnemyState.Patrol);

                        Debug.Log("Finished checking WalkTo sound");
                    }
                }
                break;

            case EnemyReaction.RunTo:
                Debug.Log($"{controller.gameObject.name} heared RunTo sound, running to sound");

                agent.speed = runSpeed;
                agent.SetDestination(SoundHeared.Position);

                if (isDistracted && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    Debug.Log("Arrived at sound");
                    agent.speed = walkSpeed;
                    distractTimer += Time.deltaTime;

                    if (distractTimer >= distractWaitTime)
                    {
                        isDistracted = false;
                        SoundHeared = null;
                        distractTimer = 0f;
                        controller.SwitchState(EnemyState.Patrol);

                        Debug.Log("Finished checking WalkTo sound");
                    }
                }
                break;
        }
    }

    public void SetSoundHeared(SoundSignal signal) => SoundHeared = signal; 
}