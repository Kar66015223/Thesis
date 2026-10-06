using UnityEngine;

public class CatchingState : IEnemyState
{
    private EnemyController ctrl;
    private PlayerStruggle player;

    public CatchingState(EnemyController ctrl, PlayerStruggle player)
    {
        this.ctrl = ctrl;
        this.player = player;
    }

    public void Enter() => player.EnterStruggle(ctrl);

    public void Update()
    {
        player.UpdateStruggle(ctrl.catchDamage, ctrl.damageCooldown);
    }
    
    public void Exit() {}
    public void OnHearSound(SoundSignal sound) { }
    public void OnSeenTarget(Transform target) { }
}