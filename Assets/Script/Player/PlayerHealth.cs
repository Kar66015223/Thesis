using System;
using UnityEngine;

/// <summary>
/// Health rules only: damage, heal, death. Reacting to death (stop moving, game-over screen)
/// is done by listeners, so this class no longer depends on PlayerController or the UI.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float _curHP;
    [SerializeField] private float maxHP = 100f;

    // The blood-effect settings stay on this component so existing Inspector values survive.
    // Ideally move them to their own "PlayerHealthEffects" component in the Editor.
    [Header("Blood Effect")]
    [SerializeField] private Material bloodFX;
    [SerializeField] private float normalVignettePower = 20f;
    [SerializeField] private float halfHPVignettePower = 10f;
    [SerializeField] private float lowHPVignettePower = 8f;
    [SerializeField] private float normalBreathFrequency = 2f;
    [SerializeField] private float lowHPBreathFrequency = 5f;
    [SerializeField, Range(0f, 1f)] private float lowHealthRatio = 0.5f;

    private HealthScreenEffect screenEffect;

    public float CurHP => _curHP;
    public float MaxHP => maxHP;
    public bool IsDead { get; private set; }

    /// <summary>(current, max)</summary>
    public event Action<float, float> HealthChanged;
    public event Action Died;

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

        _curHP = maxHP;
    }

    void OnEnable()
    {
        HealthChanged += screenEffect.Apply;
        screenEffect.Apply(_curHP, maxHP);
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

        SetHealth(_curHP - damage);
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f)
            return;

        SetHealth(_curHP + amount);
    }

    private void SetHealth(float value)
    {
        _curHP = Mathf.Clamp(value, 0f, maxHP);
        HealthChanged?.Invoke(_curHP, maxHP);

        if (_curHP <= 0f)
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

    // Replaces the old "takeDamage / heal" bools that were polled every frame.
    [ContextMenu("Debug/Take 10 Damage")]
    private void DebugTakeDamage() => TakeDamage(10f);

    [ContextMenu("Debug/Heal 10")]
    private void DebugHeal() => Heal(10f);
}
