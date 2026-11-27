using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float curHP;
    [SerializeField] private float maxHP = 100f;

    [Header("Blood Effect")]
    [SerializeField] private Material bloodFX;
    [SerializeField, Min(0.01f)] private float bloodFadeSpeed = 0.5f;

    private HealthScreenEffect screenEffect;

    public float CurHP => curHP;
    public float MaxHP => maxHP;
    public bool IsDead { get; private set; }

    public event Action<float, float> HealthChanged;
    public event Action Died;
    public event Action Revived;

    void Awake()
    {
        screenEffect = new HealthScreenEffect(bloodFX, new HealthScreenEffect.Settings
        {
            fadeSpeed = bloodFadeSpeed
        });

        curHP = maxHP;
    }

    void OnEnable()
    {
        HealthChanged += screenEffect.Apply;
        screenEffect.Apply(curHP, maxHP);
        screenEffect.Snap();
    }

    void OnDisable()
    {
        HealthChanged -= screenEffect.Apply;
        screenEffect.Restore();
    }

    void OnApplicationQuit() => screenEffect.Restore();

    void Update() => screenEffect.Tick(Time.deltaTime);
    
    public void TakeDamage(float damage)
    {
        if (IsDead || damage <= 0f)
            return;

        SetHealth(curHP - damage);
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f)
            return;

        SetHealth(curHP + amount);
    }

    public void Revive()
    {
        if (!IsDead)
            return;

        IsDead = false;
        SetHealth(maxHP);
        Revived?.Invoke();
    }

    private void SetHealth(float value)
    {
        curHP = Mathf.Clamp(value, 0f, maxHP);
        HealthChanged?.Invoke(curHP, maxHP);

        if (curHP <= 0f)
            Die();
    }

    private void Die()
    {
        if (IsDead)
            return;

        IsDead = true;
        Died?.Invoke();
        PlayerEvents.RaiseDied();
    }

    [ContextMenu("Debug/Take 10 Damage")]
    private void DebugTakeDamage() => TakeDamage(10f);

    [ContextMenu("Debug/Heal 10")]
    private void DebugHeal() => Heal(10f);
}