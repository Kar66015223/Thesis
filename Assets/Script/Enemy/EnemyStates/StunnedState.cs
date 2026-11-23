using System.Collections.Generic;
using UnityEngine;

public class StunnedState : EnemyStateBase
{
    private float timer;

    public StunnedState(EnemyController ctrl) : base(ctrl) { }

    public override EnemyState Indicator => EnemyState.Stunned;

    public override void Enter()
    {
        ctrl.Tracking.InterruptCurrentStep();

        ctrl.Agent.isStopped = true;
        timer = 0f;
        ctrl.Anim.SetStartStun();
        ctrl.Anim.SetStun(true);
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        if (timer < ctrl.StunTime)
            return;

        IReadOnlyList<Transform> visible = ctrl.Vision.VisibleTargets;
        if (visible.Count > 0)
            ctrl.ChangeState(new ChaseState(ctrl, visible[0]));
        else
            ctrl.ChangeState(new SearchState(ctrl));
    }

    public override void Exit()
    {
        ctrl.Tracking.ResumeInterruptedStep();
        ctrl.Agent.isStopped = false;
        ctrl.Anim.SetStun(false);
    }
}