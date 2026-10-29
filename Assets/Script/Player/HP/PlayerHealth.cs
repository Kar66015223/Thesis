using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float curHP;
    [SerializeField] private float maxHP = 100f;

    [Header("Blood Effect")]
    [SerializeField] private Material bloodFX;
    [SerializeField] private float normalVignettePower = 20f;
    [SerializeField] private float halfHPVignettePower = 10f;
    [SerializeField] private float lowHPVignettePower = 8f;
    [SerializeField] private float normalBreathFrequency = 2f;
    [SerializeField] private float lowHPBreathFrequency = 5f;
    [SerializeField, Range(0f, 1f)] private float lowHealthRatio = 0.5f;

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
            NormalVignettePower = normalVignettePower,
            HurtVignettePower = halfHPVignettePower,
            LowVignettePower = lowHPVignettePower,
            NormalBreathFrequency = normalBreathFrequency,
            LowBreathFrequency = lowHPBreathFrequency,
            LowHealthRatio = lowHealthRatio,
        });

        curHP = maxHP;
    }

    void OnEnable()
    {
        HealthChanged += screenEffect.Apply;
        screenEffect.Apply(curHP, maxHP);
    }

    void OnDisable()
    {
        HealthChanged -= screenEffect.Apply;
        screenEffect.Restore();
    }

    void OnApplicationQuit() => screenEffect.Restore();

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