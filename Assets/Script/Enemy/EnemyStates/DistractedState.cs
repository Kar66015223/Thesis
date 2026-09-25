using UnityEngine;

public class DistractedState : IEnemyState
{
    private EnemyController ctrl;
    private SoundSignal sound;
    private float timer;

    public DistractedState(EnemyController ctrl, SoundSignal sound)
    {
        this.ctrl = ctrl;
        this.sound = sound;
    }

    public void Enter()
    {
        ctrl.Agent.stoppingDistance = 2f;

        timer = 0f;
        if (sound.Reaction == EnemyReaction.RunTo)
            ctrl.Agent.speed = ctrl.runSpeed;
        else
            ctrl.Agent.speed = ctrl.walkSpeed;

        if (sound.Reaction == EnemyReaction.LookAt)
        {
            ctrl.Agent.updateRotation = false;
            ctrl.Agent.isStopped = true;
        }
    }

    public void Update()
    {
        if (sound.Reaction == EnemyReaction.LookAt)
            HandleLookAt();
        else
            HandleMoveTo();
    }

    private void HandleLookAt()
    {
        Vector3 direction = sound.Position - ctrl.transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            ctrl.transform.rotation = Quaternion.Slerp(
                ctrl.transform.rotation,
                Quaternion.LookRotation(direction),
                ctrl.lookAtRotationSpeed * Time.deltaTime);

            if (Vector3.Angle(ctrl.transform.forward, direction) < 5f)
                timer += Time.deltaTime;
        }

        if (timer >= ctrl.distractWaitTime)
            ctrl.ChangeState(ctrl.initialState);
    }

    private void HandleMoveTo()
    {
        ctrl.Agent.SetDestination(sound.Position);

        if (!ctrl.Agent.pathPending && ctrl.Agent.remainingDistance <= ctrl.Agent.stoppingDistance)
        {
            if (sound.Reaction == EnemyReaction.RunTo)
                ctrl.Agent.speed = ctrl.walkSpeed;

            timer += Time.deltaTime;

            if (timer >= ctrl.distractWaitTime)
                ctrl.ChangeState(ctrl.initialState);
        }
    }

    public void Exit()
    {
        ctrl.Agent.updateRotation = true;
        ctrl.Agent.isStopped = false;
    }
    
    public void OnHearSound(SoundSignal newSound) => ctrl.ChangeState(new DistractedState(ctrl, newSound));
    public void OnSeenTarget(Transform target) => ctrl.ChangeState(new ChaseState(ctrl, target));
}