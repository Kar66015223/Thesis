using UnityEngine;
using System.Collections;

public class ScannerSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    public GameObject scanVFX;
    public GameObject scanVFX2;

    public float cooldownTime = 1f;
    public bool isCooldown = false;
    public float currentCooldown = 0f;

    [Header("Audio - Scanner Only")]
    // AudioSource ตัวนี้ใช้สำหรับเสียง Scanner เท่านั้น
    public AudioSource scanAudioSource;

    public AudioClip scanStartSound;
    public AudioClip scanLoopSound;

    [Range(0f, 1f)]
    public float startVolume = 1f;

    [Range(0f, 1f)]
    public float loopVolume = 0.7f;

    public float fadeInDuration = 0.5f;

    private bool isSkillActive = false;
    private Coroutine audioCoroutine;

    void Update()
    {
        if (isCooldown)
        {
            currentCooldown -= Time.deltaTime;

            if (currentCooldown <= 0f)
            {
                isCooldown = false;
                currentCooldown = 0f;
            }
        }
    }

    public void Scan(bool flag)
    {
        // =====================================
        // กดใช้ Skill
        // =====================================
        if (flag)
        {
            if (isCooldown)
                return;

            if (!isSkillActive)
            {
                isSkillActive = true;

                // เปิด VFX ทั้งสองตัว
                if (scanVFX != null)
                {
                    scanVFX.SetActive(true);
                }

                if (scanVFX2 != null)
                {
                    scanVFX2.SetActive(true);
                }

                // เริ่มเสียง Scanner
                StartScanAudio();
            }
        }

        // =====================================
        // ปล่อย Skill
        // =====================================
        else
        {
            if (isSkillActive)
            {
                isSkillActive = false;

                isCooldown = true;
                currentCooldown = cooldownTime;
            }

            // ปิด VFX
            if (scanVFX != null)
            {
                scanVFX.SetActive(false);
            }

            if (scanVFX2 != null)
            {
                scanVFX2.SetActive(false);
            }

            // หยุดเฉพาะเสียง Scanner
            StopScanAudio();
        }
    }

    // =====================================
    // เริ่มเสียง Scanner
    // =====================================
    void StartScanAudio()
    {
        if (scanAudioSource == null)
            return;

        if (audioCoroutine != null)
        {
            StopCoroutine(audioCoroutine);
        }

        // หยุดเฉพาะ AudioSource ของ Scanner
        scanAudioSource.Stop();

        audioCoroutine = StartCoroutine(PlayScanAudio());
    }

    // =====================================
    // เสียง 1 → เสียง 2
    // =====================================
    IEnumerator PlayScanAudio()
    {
        // =====================================
        // เสียงที่ 1
        // =====================================

        if (scanStartSound != null)
        {
            scanAudioSource.clip = scanStartSound;
            scanAudioSource.loop = false;
            scanAudioSource.volume = startVolume;

            scanAudioSource.Play();

            // รอเสียงแรกจบ
            yield return new WaitWhile(
                () => scanAudioSource.isPlaying
            );
        }

        // ถ้าปล่อยปุ่มก่อนเสียงแรกจบ
        if (!isSkillActive)
            yield break;

        // =====================================
        // เสียงที่ 2
        // =====================================

        if (scanLoopSound != null)
        {
            scanAudioSource.clip = scanLoopSound;
            scanAudioSource.loop = true;

            // เริ่มจาก 0
            scanAudioSource.volume = 0f;

            scanAudioSource.Play();

            // =====================================
            // Fade In
            // =====================================

            float timer = 0f;

            while (timer < fadeInDuration)
            {
                if (!isSkillActive)
                    yield break;

                timer += Time.deltaTime;

                float t = timer / fadeInDuration;

                scanAudioSource.volume = Mathf.Lerp(
                    0f,
                    loopVolume,
                    t
                );

                yield return null;
            }

            scanAudioSource.volume = loopVolume;
        }
    }

    // =====================================
    // หยุดเฉพาะเสียง Scanner
    // =====================================
    void StopScanAudio()
    {
        if (audioCoroutine != null)
        {
            StopCoroutine(audioCoroutine);
            audioCoroutine = null;
        }

        if (scanAudioSource != null)
        {
            // หยุดเฉพาะ Scanner
            scanAudioSource.Stop();

            scanAudioSource.clip = null;
            scanAudioSource.loop = false;
            scanAudioSource.volume = 0f;
        }
    }

    // =====================================
    // Object ถูก Disable
    // =====================================
    void OnDisable()
    {
        if (scanVFX != null)
        {
            scanVFX.SetActive(false);
        }

        if (scanVFX2 != null)
        {
            scanVFX2.SetActive(false);
        }

        StopScanAudio();
    }
}