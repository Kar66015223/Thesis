using UnityEngine;

public class TargetFreedState : IEnemyState
{
    private EnemyController ctrl;

    public TargetFreedState(EnemyController ctrl)
        => this.ctrl = ctrl;

    public void Enter()
    {
        ctrl.Agent.isStopped = false;
        ctrl.Vision.DisableVision();
    }

    public void Update() { }
    public void Exit() => ctrl.Vision.EnableVision();
    public void OnHearSound(SoundSignal sound) { }
    public void OnSeenTarget(Transform target) { }
}