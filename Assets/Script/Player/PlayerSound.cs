using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SoundEmitter))]
public class PlayerSound : MonoBehaviour
{
    private AudioSource source;
    private SoundEmitter emitter;

    [Header("Footstep Audio")]
    [SerializeField] private AudioClip[] footstepClips;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)]
    private float walkVolume = 1f;

    [SerializeField, Range(0f, 1f)]
    private float runVolume = 1f;

    [Header("Random Variation")]
    [SerializeField, Range(0f, 0.1f)]
    private float pitchVariation = 0.02f;

    [SerializeField, Range(0f, 0.1f)]
    private float volumeVariation = 0.04f;

    [Header("Sound Radius")]
    public const float walkSoundRadius = 5f;
    public const float runSoundRadius = 7f;

    public const EnemyReaction walkSoundReaction = EnemyReaction.WalkTo;

    private int lastClipIndex = -1;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        emitter = GetComponent<SoundEmitter>();

        // ป้องกัน AudioSource เล่นเองตอนเริ่มเกม
        source.playOnAwake = false;
        source.loop = false;
    }

    public void PlayWalkSound()
    {
        PlayFootstep(
            walkVolume,
            walkSoundRadius,
            walkSoundReaction
        );
    }

    public void PlayRunSound()
    {
        PlayFootstep(
            runVolume,
            runSoundRadius,
            walkSoundReaction
        );
    }

    private void PlayFootstep(
        float baseVolume,
        float soundRadius,
        EnemyReaction reaction
    )
    {
        if (footstepClips == null || footstepClips.Length == 0)
            return;

        int clipIndex = GetRandomClipIndex();

        AudioClip clip = footstepClips[clipIndex];

        if (clip == null)
            return;

        // Random Volume
        float volume = baseVolume * Random.Range(
            1f - volumeVariation,
            1f + volumeVariation
        );

        // Random Pitch
        source.pitch = Random.Range(
            1f - pitchVariation,
            1f + pitchVariation
        );

        // เล่นเสียงโดยไม่ต้องเปลี่ยน source.clip
        source.PlayOneShot(clip, volume);

        // แจ้ง AI ว่าผู้เล่นสร้างเสียง
        emitter.MakeSound(soundRadius, reaction);
    }

    private int GetRandomClipIndex()
    {
        // มีแค่เสียงเดียว
        if (footstepClips.Length == 1)
        {
            lastClipIndex = 0;
            return 0;
        }

        int index;

        do
        {
            index = Random.Range(0, footstepClips.Length);
        }
        while (index == lastClipIndex);

        lastClipIndex = index;

        return index;
    }
}