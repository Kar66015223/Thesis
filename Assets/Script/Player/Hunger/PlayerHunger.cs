using UnityEngine;
using System;
using UnityEngine.Video;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerHealthRegen))]
public class PlayerHunger : MonoBehaviour
{
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float _curHunger;
    [SerializeField] private float drainRate = 0.1f;

    [Header("Drain Multipliers")]
    [SerializeField, Min(1f)] private float runMultiplier = 1.5f;
    [SerializeField, Min(1f)] private float heavyWalkMultiplier = 2f;
    [SerializeField, Min(1f)] private float heavyRunMultiplier = 3f;
    [SerializeField, Min(1f)] private float recoveringMultiplier = 5f;

    [Header("Thresholds")]
    [SerializeField, Range(0f, 1f)] private float hungryRatio = 0.5f;
    [SerializeField, Range(0f, 1f)] private float starvingRatio = 0.15f;

    public bool IsStarving => Normalized < starvingRatio;
    public event Action HungerPang;

    public float CurHunger { get => _curHunger; private set => _curHunger = Mathf.Clamp(value, 0, maxHunger); }
    public float Normalized => maxHunger > 0f ? CurHunger / maxHunger : 0f;
    public bool IsEmpty => CurHunger <= 0f;

    private PlayerController controller;
    private PlayerHealthRegen regen;

    [ContextMenu("Debug/Reduce Hunger")]
    private void DebugReduceHunger() => CurHunger -= 20f;

    [ContextMenu("Debug/Refill Hunger")]
    public void DebugFillHunger() => CurHunger += 20f;

    [ContextMenu("Debug/Half Hunger")]
    private void DebugHalfHunger() => CurHunger = 50.5f;

    [ContextMenu("Debug/Starving")]
    private void DebugStarving() => CurHunger = 15.5f;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        regen = GetComponent<PlayerHealthRegen>();
    }

    void Start() => CurHunger = maxHunger;

    void Update()
    {
        float before = Normalized;
        CurHunger -= drainRate * GetMultiplier() * Time.deltaTime;
        float after = Normalized;

        if (Crossed(before, after, hungryRatio) || Crossed(before, after, starvingRatio))
            HungerPang?.Invoke();
    }

    private static bool Crossed(float before, float after, float threshold) =>
        before >= threshold && after < threshold;

    private float GetMultiplier()
    {
        if (regen != null && regen.IsRecovering)
            return recoveringMultiplier;

        bool isHeavy = controller.Inventory.IsOverweight;

        if (controller.IsRunning)
            return isHeavy ? heavyRunMultiplier : runMultiplier;

        return isHeavy && controller.IsMoving ? heavyWalkMultiplier : 1f;
    }
}