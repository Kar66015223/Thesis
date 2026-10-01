using UnityEngine;

public class StunnedState : IEnemyState
{
    private EnemyController ctrl;
    private float timer;

    public StunnedState(EnemyController ctrl)
        => this.ctrl = ctrl;

    public void Enter()
    {
        ctrl.tracking.InterruptCurrentStep();

        ctrl.Agent.isStopped = true;
        ctrl.Vision.DisableVision();

        timer = 0f;

        GameEvent.OnAlertEnemyState?.Invoke(ctrl.transform, EnemyState.Stunned);
    }

    public void Update()
    {
        timer += Time.deltaTime;
        if (timer >= ctrl.stunTime)
        {
            ctrl.Vision.EnableVision();
            ctrl.ChangeState(ctrl.initialState);
            GameEvent.OnAlertEnemyState?.Invoke(ctrl.transform, EnemyState.None);
        }
    }
    
    public void Exit()
    {
        ctrl.Vision.EnableVision();
        ctrl.tracking.ResumeInterruptedStep();
        ctrl.Agent.isStopped = false;
    } 
    public void OnHearSound(SoundSignal sound) { }
    public void OnSeenTarget(Transform target) { }
}