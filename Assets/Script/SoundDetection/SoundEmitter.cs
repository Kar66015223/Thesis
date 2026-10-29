using System.Collections.Generic;
using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    private const int StartBufferSize = 32;
    private const string SoundproofLayerName = "Soundproof";

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
    private int soundproofMask;

    void Awake()
    {
        soundproofMask = LayerMask.GetMask(SoundproofLayerName);
        if (soundproofMask == 0)
            Debug.LogWarning($"SoundEmitter: there's no '{SoundproofLayerName}' layer, so nothing blocks sound.", this);
    }

    public void MakeSound(float radius, EnemyReaction reaction)
    {
        curRadius = radius;
        curReaction = reaction;

        Vector3 origin = transform.position;
        SoundSignal signal = new(origin, radius, reaction);
        int count;
        while ((count = Physics.OverlapSphereNonAlloc(origin, radius, overlapBuffer, hearableMask)) == overlapBuffer.Length)
            overlapBuffer = new Collider[overlapBuffer.Length * 2];

        notified.Clear();
        for (int i = 0; i < count; i++)
        {
            Collider col = overlapBuffer[i];
            if (!col.TryGetComponent(out IHearable hearable) || notified.Contains(hearable))
                continue;

            if (IsSoundproofed(origin, col.bounds.center))
                continue;

            notified.Add(hearable);
            hearable.OnHearSound(signal);
        }
    }

    private bool IsSoundproofed(Vector3 from, Vector3 to)
    {
        bool blocked = Physics.Linecast(from, to, soundproofMask, QueryTriggerInteraction.Ignore);

#if UNITY_EDITOR
        Debug.DrawLine(from, to, blocked ? Color.red : Color.green, 1f);
#endif
        return blocked;
    }

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
