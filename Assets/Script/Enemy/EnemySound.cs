using UnityEngine;

public class EnemySound : MonoBehaviour
{
    private const float MinTimeScale = 0.01f;
    private const float FootHeight = 0.1f;

    private AudioSource source;

    [Header("Footstep Audio")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField, Range(0f, 1f)] private float footstepVolume = 1f;
    private int lastClipIndex = -1;

    [Header("Random Variation")]
    [SerializeField, Range(0f, 0.1f)] private float pitchVariation = 0.02f;
    [SerializeField, Range(0f, 0.1f)] private float volumeVariation = 0.04f;

    [Header("Soundproofing")]
    [SerializeField] private float earHeight = 1.5f;
    [SerializeField] private float maxHearDistance = 30f;
    private Transform player;

    public void PlayFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0)
            return;

        if (!CanPlayerHear())
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

    private bool CanPlayerHear()
    {
        Transform target = GetPlayer();
        if (target == null)
            return true;

        Vector3 from = transform.position + Vector3.up * FootHeight;
        Vector3 ear = target.position + Vector3.up * earHeight;

        if ((ear - from).sqrMagnitude > maxHearDistance * maxHearDistance)
            return false;

        return !Soundproofing.IsBlocked(from, ear);
    }

    private Transform GetPlayer()
    {
        if (player == null)
        {
            PlayerController found = FindFirstObjectByType<PlayerController>();
            if (found != null)
                player = found.transform;
        }

        return player;
    }
}
