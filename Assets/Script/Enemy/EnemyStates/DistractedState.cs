using UnityEngine;
using UnityEngine.AI;

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
            ctrl.Movement.SetBaseSpeed(ctrl.runSpeed);
        else
            ctrl.Movement.SetBaseSpeed(ctrl.walkSpeed);

        if (sound.Reaction == EnemyReaction.LookAt)
        {
            ctrl.Agent.updateRotation = false;
            ctrl.Agent.isStopped = true;

            GameEvent.OnShowEnemyState?.Invoke(ctrl.transform, EnemyState.DistractedLook);
        }
        else
        {
            ctrl.Agent.SetDestination(sound.Position);
            GameEvent.OnShowEnemyState?.Invoke(ctrl.transform, EnemyState.DistractedMove);
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
            ctrl.ReturnToDefaultState();
    }

    private void HandleMoveTo()
    {
        bool reachedDestination =
            !ctrl.Agent.pathPending && ctrl.Agent.remainingDistance <= ctrl.Agent.stoppingDistance;

        bool isInvalidPath =
            !ctrl.Agent.pathPending && ctrl.Agent.pathStatus == NavMeshPathStatus.PathInvalid;

        if (reachedDestination || isInvalidPath)
        {
            if (sound.Reaction == EnemyReaction.RunTo)
                ctrl.Movement.SetBaseSpeed(ctrl.walkSpeed);

            timer += Time.deltaTime;

            if (timer >= ctrl.distractWaitTime)
                ctrl.ReturnToDefaultState();
        }
    }

    public void Exit()
    {
        ctrl.Agent.updateRotation = true;
        ctrl.Agent.isStopped = false;

        GameEvent.OnShowEnemyState?.Invoke(ctrl.transform, EnemyState.None);
    }
    
    public void OnHearSound(SoundSignal newSound) => ctrl.ChangeState(new DistractedState(ctrl, newSound));
    public void OnSeenTarget(Transform target) => ctrl.ChangeState(new ConfirmingState(ctrl, target));
}