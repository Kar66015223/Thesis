using UnityEngine;

/// <summary>
/// Holds the player: StartCatch grab -> the player's neck goes into the right hand and GotCaught plays ->
/// both loop (CatchedPlayerLoop / GotCaughtLoop) while the player struggles. The struggle itself runs on the
/// player (PlayerStruggle), which calls back through <see cref="IPlayerCatcher.ReleasePlayer"/> when it ends.
/// </summary>
public class CatchingState : EnemyStateBase
{
    // The struggle only starts in the loop, so if StartCatch or GotCaught never reported its end (behaviour missing
    // from the state, clip that never finishes) the player would be held forever. Move on after this long instead.
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

        // The loop leaves through Catching = false (to OutofCatch); before that only CancelCatch gets out of StartCatch.
        if (isStrangling)
            ctrl.Anim.SetCatching(false);
        else
            ctrl.Anim.CancelCatch();
    }

    // StartCatch is over: the hand is where the neck should be.
    private void HoldPlayer()
    {
        ctrl.StartCatchAnimationFinished -= HoldPlayer;
        if (isHolding)
            return;

        isHolding = true;
        phaseTimer = 0f;

        // Subscribe first: without a GotCaught animation the player reports it finished straight away.
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
