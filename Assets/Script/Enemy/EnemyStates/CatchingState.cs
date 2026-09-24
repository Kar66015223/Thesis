using UnityEngine;

public class CatchingState : IEnemyState
{
    private EnemyController ctrl;
    private PlayerController player;

    public CatchingState(EnemyController ctrl, PlayerController player)
    {
        this.ctrl = ctrl;
        this.player = player;
    }

    public void Enter() => player.SetCanMove(false);

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            player.SetCanMove(true);
            ctrl.ChangeState(new TargetFreedState(ctrl));
        }
    }
    
    public void Exit() {}
    public void OnHearSound(SoundSignal sound) { }
    public void OnSeenTarget(Transform target) { }
}