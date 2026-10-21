using System.Collections;
using UnityEngine;

/// <summary>
/// Demon Eye scanner: turns the scan VFX (which carries the ScanTrigger) on and off, plays its audio
/// and handles its cooldown. SkillManager decides *when* to toggle it.
/// Field names are unchanged so the Inspector values on the Player prefab survive.
/// </summary>
public class ScannerSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    [SerializeField] private GameObject scanVFX;
    [SerializeField] private GameObject scanVFX2;
    [SerializeField] private float cooldownTime = 1f;

    [Header("Audio - Scanner Only")]
    // AudioSource ตัวนี้ใช้สำหรับเสียง Scanner เท่านั้น (this AudioSource is for the scanner only)
    [SerializeField] private AudioSource scanAudioSource;
    [SerializeField] private AudioClip scanStartSound;
    [SerializeField] private AudioClip scanLoopSound;
    [SerializeField, Range(0f, 1f)] private float startVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float loopVolume = 0.7f;
    [SerializeField] private float fadeInDuration = 0.5f;

    private readonly Cooldown cooldown = new();
    private Coroutine audioCoroutine;

    public bool IsActive { get; private set; }
    public bool CanActivate => !IsActive && cooldown.IsReady;
    public float CooldownRemaining => cooldown.Remaining;

    void Update() => cooldown.Tick(Time.deltaTime);

    void OnDisable()
    {
        SetVFXActive(false);
        StopScanAudio();
        IsActive = false;
    }

    public void Activate()
    {
        if (!CanActivate)
            return;

        IsActive = true;
        SetVFXActive(true);
        StartScanAudio();
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            cooldown.Start(cooldownTime);
        }

        SetVFXActive(false);
        StopScanAudio();
    }

    /// <summary>Turns the scan off without starting a fresh cooldown and sets the time left, e.g. from a checkpoint.</summary>
    public void Restore(float cooldownRemaining)
    {
        IsActive = false;
        SetVFXActive(false);
        StopScanAudio();
        cooldown.Start(cooldownRemaining);
    }

    private void SetVFXActive(bool active)
    {
        if (scanVFX != null) scanVFX.SetActive(active);
        if (scanVFX2 != null) scanVFX2.SetActive(active);
    }

    private void StartScanAudio()
    {
        if (scanAudioSource == null)
            return;

        if (audioCoroutine != null)
            StopCoroutine(audioCoroutine);

        scanAudioSource.Stop();
        audioCoroutine = StartCoroutine(PlayScanAudio());
    }

    /// <summary>Start sound once, then fade the loop in (เสียง 1 -> เสียง 2).</summary>
    private IEnumerator PlayScanAudio()
    {
        if (scanStartSound != null)
        {
            PlayClip(scanStartSound, loop: false, startVolume);
            yield return new WaitWhile(() => scanAudioSource.isPlaying);
        }

        // Released before the first sound finished.
        if (!IsActive || scanLoopSound == null)
            yield break;

        PlayClip(scanLoopSound, loop: true, volume: 0f);

        for (float timer = 0f; timer < fadeInDuration; timer += Time.deltaTime)
        {
            if (!IsActive)
                yield break;

            scanAudioSource.volume = Mathf.Lerp(0f, loopVolume, timer / fadeInDuration);
            yield return null;
        }

        scanAudioSource.volume = loopVolume;
    }

    private void PlayClip(AudioClip clip, bool loop, float volume)
    {
        scanAudioSource.clip = clip;
        scanAudioSource.loop = loop;
        scanAudioSource.volume = volume;
        scanAudioSource.Play();
    }

    private void StopScanAudio()
    {
        if (audioCoroutine != null)
        {
            StopCoroutine(audioCoroutine);
            audioCoroutine = null;
        }

        if (scanAudioSource == null)
            return;

        scanAudioSource.Stop();
        scanAudioSource.clip = null;
        scanAudioSource.loop = false;
        scanAudioSource.volume = 0f;
    }
}
