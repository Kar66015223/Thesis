using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerStruggle : MonoBehaviour
{
    [SerializeField] private float struggleThreshold = 100f;
    [SerializeField] private float struggleValue = 10f;
    private float struggleProgress = 0f;

    private float damageCooldownTimer;

    public bool IsCatched { get; private set; } = false;
    public EnemyController CatchingEnemy { get; private set; }

    private PlayerHealth hp;
    private PlayerInputHandler inputHandler;

    void Awake()
    {
        hp = GetComponent<PlayerHealth>();
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    void OnEnable()
    {
        inputHandler.OnStruggleInput += OnStruggleInput;
    }

    void OnDisable()
    {
        inputHandler.OnStruggleInput -= OnStruggleInput;
    }

    public void EnterStruggle(EnemyController eCtrl)
    {
        IsCatched = true;
        GameEvent.OnSwitchActionMap?.Invoke(PlayerConstants.ACTIONMAP_STRUGGLE);
        CatchingEnemy = eCtrl;

        GameEvent.OnToggleStruggleUI?.Invoke(true);
    }

    public void UpdateStruggle(float catchDamage, float damageCooldown)
    {
        damageCooldownTimer += Time.deltaTime;

        if (damageCooldownTimer >= damageCooldown)
        {
            damageCooldownTimer = 0f;
            hp.TakeDamage(catchDamage);
            CatchingEnemy.ShakeScreen();
        }

        if (struggleProgress >= struggleThreshold || hp.IsDead)
        {
            EndStruggle(hp.IsDead);
            GameEvent.OnToggleStruggleUI?.Invoke(false);
        }
    }
    
    private void OnStruggleInput()
    {
        AddStruggle(struggleValue);
    }
    
    public void AddStruggle(float amount)
    {
        struggleProgress += amount;
        GameEvent.OnStruggleProgressChanged?.Invoke(Mathf.Clamp01(struggleProgress / struggleThreshold));
    }
    
    public void EndStruggle(bool playerDead)
    {
        IsCatched = false;
        GameEvent.OnSwitchActionMap?.Invoke(PlayerConstants.ACTIONMAP_PLAYER);

        if (!playerDead)
        {
            CatchingEnemy.ChangeState(new StunnedState(CatchingEnemy));
            CatchingEnemy = null;
        }
        else
        {
            CatchingEnemy.ReturnToDefaultState();
            CatchingEnemy = null;
        }

        struggleProgress = 0f;
    }
}