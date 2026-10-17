using UnityEngine;
using System;

/// <summary>
/// Stamina rules only. Other components tell it what the player is doing (SetMoving / SetRunIntent)
/// and listen to <see cref="Exhausted"/> instead of stamina calling sound or camera code itself.
/// </summary>
public class PlayerStamina : MonoBehaviour
{
    [SerializeField] private float maxStamina = 100.0f;
    [SerializeField] private float _curStamina;
    [SerializeField] private float runDrainRate = 20f;
    [SerializeField] private float regenRate = 15f;
    [Range(0, 1)][SerializeField] private float recoveryThreshold = 0.25f;

    private bool isMoving;
    private bool wantsToRun;

    public float CurStamina
    {
        get => _curStamina;
        private set => _curStamina = Mathf.Clamp(value, 0, maxStamina);
    }

    public float Normalized => maxStamina > 0f ? CurStamina / maxStamina : 0f;
    public bool IsRunning { get; private set; }
    public bool IsExhausted { get; private set; }

    public event Action Exhausted;
    public event Action Recovered;

    void Start() => CurStamina = maxStamina;

    void Update() => Tick(Time.deltaTime);

    public void SetMoving(bool moving) => isMoving = moving;
    public void SetRunIntent(bool wantsRun) => wantsToRun = wantsRun;

    private void Tick(float deltaTime)
    {
        IsRunning = isMoving && wantsToRun && !IsExhausted && CurStamina > 0f;

        if (IsRunning)
        {
            CurStamina -= runDrainRate * deltaTime;

            if (CurStamina <= 0f)
            {
                IsRunning = false;
                IsExhausted = true;
                Exhausted?.Invoke();
            }
        }
        else
        {
            CurStamina += regenRate * deltaTime;

            if (IsExhausted && CurStamina >= maxStamina * recoveryThreshold)
            {
                IsExhausted = false;
                Recovered?.Invoke();
            }
        }
    }
}