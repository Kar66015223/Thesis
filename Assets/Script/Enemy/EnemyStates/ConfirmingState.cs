using UnityEngine;

/// <summary>"Was that the player?" - stops, stares, and either confirms (Chase) or gives up.</summary>
public class ConfirmingState : EnemyStateBase
{
    private readonly Transform target;
    private float giveUpTimer;

    public ConfirmingState(EnemyController ctrl, Transform target) : base(ctrl) => this.target = target;

    public override EnemyState Indicator => EnemyState.Confirming;

    public override void Enter()
    {
        ctrl.Tracking.InterruptCurrentStep();

        ctrl.Movement.SetBaseSpeed(ctrl.WalkSpeed);
        ctrl.Agent.isStopped = true;
        ctrl.Agent.updateRotation = false;
    }

    public override void Update()
    {
        if (target == null)
            return;

        FaceInstantly(target.position);

        giveUpTimer += Time.deltaTime;
        if (giveUpTimer >= ctrl.ConfirmGiveUpTime)
            ctrl.ReturnToDefaultState();
    }

    public override void Exit()
    {
        ctrl.Agent.updateRotation = true;
        ctrl.Agent.isStopped = false;

        ctrl.Tracking.ResumeInterruptedStep();
    }

    public override void OnSeenTarget(Transform seenTarget) => ctrl.ChangeState(new ChaseState(ctrl, seenTarget));
}
