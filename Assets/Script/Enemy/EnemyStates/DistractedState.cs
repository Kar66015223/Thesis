using UnityEngine;

public class DistractedState : EnemyStateBase
{
    private const float FacingToleranceDegrees = 5f;

    private readonly SoundSignal sound;
    private float timer;

    public DistractedState(EnemyController ctrl, SoundSignal sound) : base(ctrl) => this.sound = sound;

    private bool IsLookOnly => sound.Reaction == EnemyReaction.LookAt;

    public override EnemyState Indicator => IsLookOnly ? EnemyState.DistractedLook : EnemyState.DistractedMove;

    public override void Enter()
    {
        ctrl.Agent.stoppingDistance = ApproachStoppingDistance;
        timer = 0f;

        ctrl.Movement.SetBaseSpeed(sound.Reaction == EnemyReaction.RunTo ? ctrl.RunSpeed : ctrl.WalkSpeed);

        if (IsLookOnly)
        {
            ctrl.Agent.updateRotation = false;
            ctrl.Agent.isStopped = true;
        }
        else
        {
            ctrl.SetDestination(sound.Position);
        }
    }

    public override void Update()
    {
        if (IsLookOnly)
            UpdateLookAt();
        else
            UpdateMoveTo();
    }

    public override void Exit()
    {
        ctrl.Agent.updateRotation = true;
        ctrl.Agent.isStopped = false;
    }

    public override void OnHearSound(SoundSignal newSound) => ctrl.ChangeState(new DistractedState(ctrl, newSound));
    public override void OnSeenTarget(Transform target) => ctrl.ChangeState(new ConfirmingState(ctrl, target));

    private void UpdateLookAt()
    {
        Vector3 direction = FlatDirectionTo(sound.Position);

        if (direction != Vector3.zero)
        {
            RotateTowards(Quaternion.LookRotation(direction));

            if (Vector3.Angle(ctrl.transform.forward, direction) < FacingToleranceDegrees)
                timer += Time.deltaTime;
        }

        if (timer >= ctrl.DistractWaitTime)
            ctrl.ReturnToDefaultState();
    }

    private void UpdateMoveTo()
    {
        EnemyDoorNavigation doorNav = ctrl.DoorNavigation;
        bool finished = doorNav.HasArrived || (doorNav.IsBlocked && HasReachedDestination());
        if (!finished)
            return;

        if (sound.Reaction == EnemyReaction.RunTo)
            ctrl.Movement.SetBaseSpeed(ctrl.WalkSpeed);

        ctrl.Agent.updateRotation = false;
        Vector3 direction = FlatDirectionTo(sound.Position);
        if (direction != Vector3.zero)
            RotateTowards(Quaternion.LookRotation(direction));

        timer += Time.deltaTime;
        if (timer >= ctrl.DistractWaitTime)
            ctrl.ReturnToDefaultState();
    }
}
