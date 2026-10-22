using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Noisier floor: while the player stands inside this trigger, <see cref="PlayerSound"/> plays
/// <see cref="FootstepClips"/> and enemies hear each step from <see cref="SoundMultiplier"/> times further away.
/// Use a Box, Sphere or Capsule collider (or a convex Mesh), reaching down to the floor.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LouderFootstepArea : MonoBehaviour
{
    // Registry so PlayerSound can ask which area it's in on each step without a physics query.
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

    /// <summary>
    /// Asked on every footstep instead of tracking OnTriggerEnter/Exit: doors, hiding spots and checkpoint respawns
    /// move the player by turning the CharacterController off and on, and Unity sends no exit for a disabled collider,
    /// so the player could leave through a door and keep the loud footsteps.
    /// </summary>
    public bool Contains(Vector3 point) => area.enabled && area.ClosestPoint(point) == point;
}
