using System.Security.Cryptography;
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerHealthRegen : MonoBehaviour
{
    [SerializeField] private float delayAfterFreed = 5f;
    [SerializeField] private float healPerSecond = 5f;

    private PlayerHealth health;
    private PlayerStruggle struggle;
    private PlayerHunger hunger;
    private float delayTimer = -1f;

    public bool IsRecovering { get; private set; }

    void Awake()
    {
        health = GetComponent<PlayerHealth>();
        struggle = GetComponent<PlayerStruggle>();
        hunger = GetComponent<PlayerHunger>();
    }

    void OnEnable()
    {
        struggle.Freed += HandleFreed;
        struggle.Caught += Cancel;
        health.Died += Cancel;
    }

    void OnDisable()
    {
        struggle.Freed -= HandleFreed;
        struggle.Caught -= Cancel;
        health.Died -= Cancel;
    }

    void Update()
    {
        if (delayTimer >= 0f)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer > 0f)
                return;

            delayTimer = -1f;
            IsRecovering = true;
        }

        if (!IsRecovering)
            return;

        if (health.CurHP >= health.MaxHP || hunger.IsEmpty)
        {
            IsRecovering = false;
            return;
        }

        health.Heal(healPerSecond * Time.deltaTime);
    }

    private void HandleFreed()
    {
        IsRecovering = false;
        delayTimer = delayAfterFreed;
    }

    private void Cancel()
    {
        IsRecovering = false;
        delayTimer = -1f;
    }
}