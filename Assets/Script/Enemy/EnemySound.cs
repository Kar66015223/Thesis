using UnityEngine;

public class EnemySound : MonoBehaviour
{
    private const float MinTimeScale = 0.01f;

    private AudioSource source;

    [Header("Footstep Audio")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField, Range(0f, 1f)] private float footstepVolume = 1f;

    [Header("Random Variation")]
    [SerializeField, Range(0f, 0.1f)] private float pitchVariation = 0.02f;
    [SerializeField, Range(0f, 0.1f)] private float volumeVariation = 0.04f;

    private int lastClipIndex = -1;

    public void PlayFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0)
            return;

        AudioClip clip = footstepClips[GetRandomClipIndex(footstepClips.Length)];
        if (clip == null)
            return;

        float volume = footstepVolume * Random.Range(1f - volumeVariation, 1f + volumeVariation);
        float pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);

        PlayClipAtPoint(clip, transform.position, volume, pitch);
    }

    private static void PlayClipAtPoint(AudioClip clip, Vector3 position, float volume, float pitch)
    {
        GameObject oneShot = new("One shot audio");
        oneShot.transform.position = position;

        AudioSource oneShotSource = oneShot.AddComponent<AudioSource>();
        oneShotSource.clip = clip;
        oneShotSource.spatialBlend = 1f;
        oneShotSource.volume = volume;
        oneShotSource.pitch = pitch;
        oneShotSource.Play();

        Destroy(oneShot, clip.length / pitch * Mathf.Max(Time.timeScale, MinTimeScale));
    }

    private int GetRandomClipIndex(int clipCount)
    {
        if (clipCount == 1)
            return lastClipIndex = 0;

        if (lastClipIndex < 0 || lastClipIndex >= clipCount)
            return lastClipIndex = Random.Range(0, clipCount);

        int index = Random.Range(0, clipCount - 1);
        if (index >= lastClipIndex)
            index++;

        return lastClipIndex = index;
    }
}
