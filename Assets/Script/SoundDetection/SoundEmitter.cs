using System.Collections.Generic;
using UnityEngine;

/// <summary>Broadcasts a noise to every <see cref="IHearable"/> in range.</summary>
public class SoundEmitter : MonoBehaviour
{
    private const int MaxColliders = 32;

    [SerializeField] private LayerMask hearableMask = ~0;

    [Header("Debug")]
    [Tooltip("Radius of the last sound (drawn as a gizmo).")]
    [SerializeField] private float curRadius = 0f;
    [SerializeField] private EnemyReaction curReaction;
    [SerializeField] private float debugRadius = 10f;

    private readonly Collider[] overlapBuffer = new Collider[MaxColliders];
    private readonly HashSet<IHearable> notified = new();

    public void MakeSound(float radius, EnemyReaction reaction)
    {
        curRadius = radius;
        curReaction = reaction;

        SoundSignal signal = new(transform.position, radius, reaction);
        int count = Physics.OverlapSphereNonAlloc(transform.position, radius, overlapBuffer, hearableMask);

        // A listener with several colliders should still only hear the sound once.
        notified.Clear();
        for (int i = 0; i < count; i++)
        {
            if (overlapBuffer[i].TryGetComponent(out IHearable hearable) && notified.Add(hearable))
                hearable.OnHearSound(signal);
        }
    }

    // Replaces the old "makeSound" bool that was polled every frame.
    [ContextMenu("Debug/Make Sound")]
    private void DebugMakeSound() => MakeSound(debugRadius, curReaction);

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, curRadius);
    }
#endif
}
