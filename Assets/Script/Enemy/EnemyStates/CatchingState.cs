/// <summary>
/// Holds the player. The struggle loop itself runs on the player (PlayerStruggle), which calls
/// back through <see cref="IPlayerCatcher.ReleasePlayer"/> when it ends.
/// </summary>
public class CatchingState : EnemyStateBase
{
    private readonly PlayerStruggle player;

    public CatchingState(EnemyController ctrl, PlayerStruggle player) : base(ctrl) => this.player = player;

    public override void Enter()
    {
        if (!player.TryBeginStruggle(ctrl))
        {
            ctrl.ReturnToDefaultState();
            return;
        }

        ctrl.Anim.SetStartCatch();
        ctrl.Anim.SetCatching(true);
    }
}