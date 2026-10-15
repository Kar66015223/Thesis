using UnityEngine;

/// <summary>
/// The player's side of being caught: takes periodic damage, counts button mashes,
/// and tells the catcher how it ended. It runs its own loop and only knows the catcher
/// through <see cref="IPlayerCatcher"/>, so it no longer creates enemy states itself.
/// </summary>
[RequireComponent(typeof(PlayerHealth))]
public class PlayerStruggle : MonoBehaviour
{
    [SerializeField] private float struggleThreshold = 100f;
    [SerializeField] private float struggleValue = 10f;

    private float struggleProgress;
    private float damageTimer;

    private PlayerHealth hp;
    private PlayerInputHandler inputHandler;

    public bool IsCaught { get; private set; }
    public IPlayerCatcher CatchingEnemy { get; private set; }

    void Awake()
    {
        hp = GetComponent<PlayerHealth>();
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    void OnEnable()
    {
        if (inputHandler != null)
            inputHandler.OnStruggleInput += HandleStruggleInput;
    }

    void OnDisable()
    {
        if (inputHandler != null)
            inputHandler.OnStruggleInput -= HandleStruggleInput;

        if (IsCaught)
            InputModeService.SetStruggling(false);
    }

    void Update()
    {
        if (!IsCaught)
            return;

        damageTimer += Time.deltaTime;
        if (damageTimer >= CatchingEnemy.DamageInterval)
        {
            damageTimer = 0f;
            hp.TakeDamage(CatchingEnemy.CatchDamage);
            CatchingEnemy.OnCatchDamageDealt();
        }

        if (hp.IsDead)
            EndStruggle(playerEscaped: false);
        else if (struggleProgress >= struggleThreshold)
            EndStruggle(playerEscaped: true);
    }

    /// <summary>Returns false if the player can't be caught right now (already caught or dead).</summary>
    public bool TryBeginStruggle(IPlayerCatcher catcher)
    {
        if (catcher == null || IsCaught || hp.IsDead)
            return false;

        IsCaught = true;
        CatchingEnemy = catcher;
        struggleProgress = 0f;
        damageTimer = 0f;

        InputModeService.SetStruggling(true);
        PlayerEvents.RaiseStruggleToggled(true);
        PlayerEvents.RaiseStruggleProgressChanged(0f);
        return true;
    }

    private void HandleStruggleInput()
    {
        if (!IsCaught)
            return;

        struggleProgress += struggleValue;
        PlayerEvents.RaiseStruggleProgressChanged(Mathf.Clamp01(struggleProgress / struggleThreshold));
    }

    private void EndStruggle(bool playerEscaped)
    {
        IPlayerCatcher catcher = CatchingEnemy;

        IsCaught = false;
        CatchingEnemy = null;
        struggleProgress = 0f;

        InputModeService.SetStruggling(false);
        PlayerEvents.RaiseStruggleToggled(false);

        catcher.ReleasePlayer(playerEscaped);
    }
}
