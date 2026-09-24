using Unity.VisualScripting;
using UnityEngine;

public class CatchingState : IEnemyState
{
    private EnemyController ctrl;
    private PlayerController player;

    private bool isCatching = false;

    public CatchingState(EnemyController ctrl, PlayerController player)
    {
        this.ctrl = ctrl;
        this.player = player;
    }

    public void Enter() => player.SetCanMove(false);

    public void Update()
    {
        ctrl.escapePrompt.text = "[Spacebar] escape";
        isCatching = true;

        if (Input.GetKeyDown(KeyCode.Space) && isCatching)
        {
            isCatching = false;
            ctrl.escapePrompt.text = "";

            player.SetCanMove(true);
            ctrl.ChangeState(new StunnedState(ctrl));
        }
    }
    
    public void Exit() {}
    public void OnHearSound(SoundSignal sound) { }
    public void OnSeenTarget(Transform target) { }
}