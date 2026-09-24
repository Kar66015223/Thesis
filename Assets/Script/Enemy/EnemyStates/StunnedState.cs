using UnityEngine;

public class StunnedState : IEnemyState
{
    private EnemyController ctrl;
    private float timer;

    public StunnedState(EnemyController ctrl)
        => this.ctrl = ctrl;

    public void Enter()
    {
        ctrl.Agent.isStopped = false;
        ctrl.Vision.DisableVision();

        timer = 0f;
    }

    public void Update()
    {
        timer += Time.deltaTime;
        if (timer >= ctrl.stunTime)
        {
            ctrl.Vision.EnableVision();
            ctrl.ChangeState(new PatrolState(ctrl));
        }
    }
    
    public void Exit() => ctrl.Vision.EnableVision();
    public void OnHearSound(SoundSignal sound) { }
    public void OnSeenTarget(Transform target) { }
}