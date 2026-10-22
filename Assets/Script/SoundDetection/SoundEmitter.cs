using System.Collections.Generic;
using UnityEngine;

/// <summary>Broadcasts a noise to every <see cref="IHearable"/> in range.</summary>
public class SoundEmitter : MonoBehaviour
{
    private const int StartBufferSize = 32;

    [Tooltip("Layers that can hear. Set it to just the listeners' layer (Enemy): with Everything, " +
             "every wall and chair in range is checked too.")]
    [SerializeField] private LayerMask hearableMask = ~0;

    [Header("Debug")]
    [Tooltip("Radius of the last sound (drawn as a gizmo).")]
    [SerializeField] private float curRadius = 0f;
    [SerializeField] private EnemyReaction curReaction;
    [SerializeField] private float debugRadius = 10f;

    private Collider[] overlapBuffer = new Collider[StartBufferSize];
    private readonly HashSet<IHearable> notified = new();

    public void MakeSound(float radius, EnemyReaction reaction)
    {
        curRadius = radius;
        curReaction = reaction;

        SoundSignal signal = new(transform.position, radius, reaction);
        // NonAlloc silently drops whatever doesn't fit, so a full buffer means listeners may have been cut: grow and ask again.
        int count;
        while ((count = Physics.OverlapSphereNonAlloc(transform.position, radius, overlapBuffer, hearableMask)) == overlapBuffer.Length)
            overlapBuffer = new Collider[overlapBuffer.Length * 2];

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
