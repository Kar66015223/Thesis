using UnityEngine;

/// <summary>Blind and frozen for <see cref="EnemyController.StunTime"/> seconds.</summary>
public class StunnedState : EnemyStateBase
{
    private float timer;

    public StunnedState(EnemyController ctrl) : base(ctrl) { }

    public override EnemyState Indicator => EnemyState.Stunned;

    public override void Enter()
    {
        ctrl.Tracking.InterruptCurrentStep();

        ctrl.Agent.isStopped = true;
        ctrl.Vision.DisableVision();
        timer = 0f;
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        if (timer >= ctrl.StunTime)
            ctrl.ReturnToDefaultState();
    }

    public override void Exit()
    {
        ctrl.Vision.EnableVision();
        ctrl.Tracking.ResumeInterruptedStep();
        ctrl.Agent.isStopped = false;
    }
}
