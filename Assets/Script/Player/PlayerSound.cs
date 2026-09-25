using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SoundEmitter))]
public class PlayerSound : MonoBehaviour
{
    private AudioSource source;
    private SoundEmitter emitter;

    [SerializeField] private AudioClip walkClip;
    public const float walkSoundRadius = 5f;
    public const EnemyReaction walkSoundReaction = EnemyReaction.WalkTo;

    public const float runSoundRadius = 7f;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        emitter = GetComponent<SoundEmitter>();
    }

    public void PlayWalkSound()
    {
        if (source.isPlaying)
            source.Stop();

        source.clip = walkClip;
        source.Play();

        emitter.MakeSound(walkSoundRadius, walkSoundReaction);
    }
    
    public void PlayRunSound()
    {
        if (source.isPlaying)
            source.Stop();

        source.clip = walkClip;
        source.Play();

        emitter.MakeSound(runSoundRadius, walkSoundReaction);
    }
}