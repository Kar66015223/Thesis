public interface IPlayerCatcher
{
    float CatchDamage { get; }
    float DamageInterval { get; }

    void OnCatchDamageDealt();
    void ReleasePlayer(bool playerEscaped);
}