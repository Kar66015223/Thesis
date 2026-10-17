using Unity.VisualScripting;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(PlayerInputHandler))]
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
        inputHandler.OnStruggleInput += HandleStruggleInput;
    }

    void OnDisable()
    {
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
        PlayerEvents.RaiseStruggleProgressChanged(Mathf.Clamp01(struggleProgress / struggleProgress));
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