using System;
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

    private PlayerController ctrl;
    private PlayerHealth hp;

    public event Action<bool> OnToggleStruggleUI;
    public event Action<float> OnStruggleProgressChanged;

    void Awake()
    {
        ctrl = GetComponent<PlayerController>();
        hp = GetComponent<PlayerHealth>();
    }

    public void EnterStruggle(EnemyController eCtrl)
    {
        IsCatched = true;
        ctrl.SetCanMove(false);
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

        if (Input.GetKeyDown(KeyCode.Space))
            AddStruggle(struggleValue);

        if (struggleProgress >= struggleThreshold || hp.IsDead)
        {
            EndStruggle(hp.IsDead);
            GameEvent.OnToggleStruggleUI?.Invoke(false);
        }
    }
    
    public void AddStruggle(float amount)
    {
        struggleProgress += amount;
        OnStruggleProgressChanged?.Invoke(amount);
    }
    
    public void EndStruggle(bool playerDead)
    {
        IsCatched = false;
        ctrl.SetCanMove(true);

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