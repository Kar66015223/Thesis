/// <summary>Something that can grab the player. The catcher decides what happens to itself on release.</summary>
public interface IPlayerCatcher
{
    float CatchDamage { get; }
    float DamageInterval { get; }

    /// <summary>Feedback hook (e.g. screen shake) each time catch damage is dealt.</summary>
    void OnCatchDamageDealt();
    void ReleasePlayer(bool playerEscaped);
}
