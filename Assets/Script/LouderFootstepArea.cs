using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LouderFootstepArea : MonoBehaviour
{
    private static readonly List<LouderFootstepArea> activeAreas = new();
    public static IReadOnlyList<LouderFootstepArea> ActiveAreas => activeAreas;

    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float soundMultiplier = 1.5f;

    private Collider area;

    public AudioClip[] FootstepClips => footstepClips;
    public float SoundMultiplier => soundMultiplier;

    void Reset() => GetComponent<Collider>().isTrigger = true;

    void Awake() => area = GetComponent<Collider>();

    void OnEnable() => activeAreas.Add(this);
    void OnDisable() => activeAreas.Remove(this);

    public bool Contains(Vector3 point) => area.enabled && area.ClosestPoint(point) == point;
}
