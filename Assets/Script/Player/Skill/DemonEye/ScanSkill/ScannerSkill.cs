using System;
using System.Collections;
using UnityEngine;

public class ScannerSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    [SerializeField] private GameObject scanVFX;
    [SerializeField] private GameObject scanVFX2;
    [SerializeField] private float cooldownTime = 1f;

    [SerializeField] private float maxResource = 100f;
    [SerializeField] private float _curResource;
    public float CurResource
    {
        get => _curResource;
        private set => _curResource = Mathf.Clamp(value, 0, maxResource);
    }
    [SerializeField] private float drainRate = 20f;
    [SerializeField] private float regenRate = 15f;
    [Range(0, 1)][SerializeField] private float recoveryThreshold;

    public float Normalized => maxResource > 0f ? CurResource / maxResource : 0f;
    public event Action<float> OnResourceChanged;

    [Header("Audio - Scanner Only")]
    [SerializeField] private AudioSource scanAudioSource;
    [SerializeField] private AudioClip scanStartSound;
    [SerializeField] private AudioClip scanLoopSound;
    [SerializeField, Range(0f, 1f)] private float startVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float loopVolume = 0.7f;
    [SerializeField] private float fadeInDuration = 0.5f;

    private readonly Cooldown cooldown = new();
    private Coroutine audioCoroutine;

    public bool IsActive { get; private set; }
    public bool OutOfResource { get; private set; } = false;
    public event Action OnOutOfResource;
    public event Action Recovered;
    
    public bool CanActivate => !IsActive && cooldown.IsReady && CurResource > 0 && !OutOfResource;
    public float CooldownRemaining => cooldown.Remaining;

    void Start() => CurResource = maxResource;
    void Update() => Tick(Time.deltaTime);

    private void Tick(float deltaTime)
    {
        cooldown.Tick(deltaTime);

        if (IsActive)
        {
            CurResource -= drainRate * deltaTime;
            OnResourceChanged?.Invoke(Normalized);

            if (CurResource <= 0f)
            {
                OutOfResource = true;
                OnOutOfResource?.Invoke();
            }
        }
        else
        {
            CurResource += regenRate * deltaTime;
            OnResourceChanged?.Invoke(Normalized);

            if(OutOfResource && CurResource >= maxResource * recoveryThreshold)
            {
                OutOfResource = false;
                Recovered?.Invoke();
            }
        }
    }

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

    private IEnumerator PlayScanAudio()
    {
        if (scanStartSound != null)
        {
            PlayClip(scanStartSound, loop: false, startVolume);
            yield return new WaitWhile(() => scanAudioSource.isPlaying);
        }

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
