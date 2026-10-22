using UnityEngine;

/// <summary>
/// Footstep and breathing audio, plus the matching noise that enemies can hear.
/// Footsteps are triggered by animation events (PlayerAnimationEventHandler);
/// the tired sound reacts to <see cref="PlayerStamina.Exhausted"/>.
/// </summary>
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SoundEmitter))]
public class PlayerSound : MonoBehaviour
{
    // Checked a little above the feet so an area whose bottom sits exactly on the floor still counts.
    private const float FootstepProbeHeight = 0.2f;

    [Header("Footstep Audio")]
    [Tooltip("Normal footsteps. A LouderFootstepArea the player stands in replaces these with its own clips.")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioClip tiredClip;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float walkVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float runVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float tiredVolume = 1f;

    [Header("Random Variation")]
    [SerializeField, Range(0f, 0.1f)] private float pitchVariation = 0.02f;
    [SerializeField, Range(0f, 0.1f)] private float volumeVariation = 0.04f;

    [Header("Noise (heard by enemies)")]
    [SerializeField] private float walkSoundRadius = 5f;
    [SerializeField] private float runSoundRadius = 7f;
    [SerializeField] private EnemyReaction footstepReaction = EnemyReaction.WalkTo;

    private AudioSource source;
    private SoundEmitter emitter;
    private PlayerStamina stamina;
    private int lastClipIndex = -1;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        emitter = GetComponent<SoundEmitter>();
        stamina = GetComponent<PlayerStamina>();

        // Don't let the AudioSource play by itself when the game starts.
        source.playOnAwake = false;
        source.loop = false;
    }

    void OnEnable()
    {
        if (stamina != null)
            stamina.Exhausted += PlayTiredSound;
    }

    void OnDisable()
    {
        if (stamina != null)
            stamina.Exhausted -= PlayTiredSound;
    }

    public void PlayWalkSound() => PlayFootstep(walkVolume, walkSoundRadius);
    public void PlayRunSound() => PlayFootstep(runVolume, runSoundRadius);

    public void PlayTiredSound()
    {
        if (tiredClip == null)
            return;

        source.pitch = 1f;
        source.PlayOneShot(tiredClip, tiredVolume);
    }

    private void PlayFootstep(float baseVolume, float noiseRadius)
    {
        LouderFootstepArea area = FindFootstepArea();

        // An area without clips of its own still makes the step louder for enemies.
        AudioClip[] clips = area != null && area.FootstepClips != null && area.FootstepClips.Length > 0
            ? area.FootstepClips
            : footstepClips;

        if (clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[GetRandomClipIndex(clips.Length)];
        if (clip == null)
            return;

        float volume = baseVolume * Random.Range(1f - volumeVariation, 1f + volumeVariation);
        source.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);

        // PlayOneShot doesn't need to swap source.clip.
        source.PlayOneShot(clip, volume);

        // Let the AI know the player made a noise.
        float radius = area != null ? noiseRadius * area.SoundMultiplier : noiseRadius;
        emitter.MakeSound(radius, footstepReaction);
    }

    private LouderFootstepArea FindFootstepArea()
    {
        Vector3 probe = transform.position + Vector3.up * FootstepProbeHeight;
        foreach (LouderFootstepArea area in LouderFootstepArea.ActiveAreas)
        {
            if (area.Contains(probe))
                return area;
        }

        return null;
    }

    /// <summary>Random clip that never repeats the previous one (no retry loop needed).</summary>
    private int GetRandomClipIndex(int clipCount)
    {
        if (clipCount == 1)
            return lastClipIndex = 0;

        // Also covers switching to a shorter clip list when entering or leaving an area.
        if (lastClipIndex < 0 || lastClipIndex >= clipCount)
            return lastClipIndex = Random.Range(0, clipCount);

        int index = Random.Range(0, clipCount - 1);
        if (index >= lastClipIndex)
            index++;

        return lastClipIndex = index;
    }
}
