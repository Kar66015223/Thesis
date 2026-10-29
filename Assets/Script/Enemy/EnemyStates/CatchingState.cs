using UnityEngine;

public class CatchingState : EnemyStateBase
{
    private const float MaxGrabPhaseTime = 5f;

    private readonly PlayerStruggle player;
    private bool hasCaught;
    private bool isHolding;
    private bool isStrangling;
    private float phaseTimer;

    public CatchingState(EnemyController ctrl, PlayerStruggle player) : base(ctrl) => this.player = player;

    public override void Enter()
    {
        if (!player.TryBeginStruggle(ctrl))
        {
            ctrl.ReturnToDefaultState();
            return;
        }

        hasCaught = true;
        phaseTimer = 0f;
        ctrl.Agent.isStopped = true;
        FaceInstantly(player.transform.position);

        ctrl.StartCatchAnimationFinished += HoldPlayer;
        ctrl.Anim.SetStartCatch();
    }

    public override void Update()
    {
        if (!hasCaught || isStrangling)
            return;

        phaseTimer += Time.deltaTime;
        if (phaseTimer < MaxGrabPhaseTime)
            return;

        if (isHolding)
            StartStrangling();
        else
            HoldPlayer();
    }

    public override void Exit()
    {
        ctrl.StartCatchAnimationFinished -= HoldPlayer;
        player.GotCaughtFinished -= StartStrangling;

        if (!hasCaught)
            return;

        if (isStrangling)
            ctrl.Anim.SetCatching(false);
        else
            ctrl.Anim.CancelCatch();
    }

    private void HoldPlayer()
    {
        ctrl.StartCatchAnimationFinished -= HoldPlayer;
        if (isHolding)
            return;

        isHolding = true;
        phaseTimer = 0f;

        player.GotCaughtFinished += StartStrangling;
        player.BeginHold(ctrl.RightHand, ctrl.transform.position);
    }

    private void StartStrangling()
    {
        player.GotCaughtFinished -= StartStrangling;
        if (isStrangling)
            return;

        isStrangling = true;
        ctrl.Anim.SetCatching(true);
        player.StartCaughtLoop();
    }
}
