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

    [Header("Audio")]
    public AudioSource audioSource;

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

                // เปิด VFX ทั้งสองตัวพร้อมกันทันที
                if (scanVFX != null)
                {
                    scanVFX.SetActive(true);
                }

                if (scanVFX2 != null)
                {
                    scanVFX2.SetActive(true);
                }

                // เริ่มเสียง
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

            // ปิด VFX ทั้งสองตัว
            if (scanVFX != null)
            {
                scanVFX.SetActive(false);
            }

            if (scanVFX2 != null)
            {
                scanVFX2.SetActive(false);
            }

            // หยุดเสียง
            StopScanAudio();
        }
    }

    // =====================================
    // เริ่มระบบเสียง
    // =====================================
    void StartScanAudio()
    {
        if (audioSource == null)
            return;

        if (audioCoroutine != null)
        {
            StopCoroutine(audioCoroutine);
        }

        audioSource.Stop();

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
            audioSource.clip = scanStartSound;
            audioSource.loop = false;
            audioSource.volume = startVolume;

            audioSource.Play();

            // รอเสียงแรกเล่นจนจบ
            yield return new WaitWhile(
                () => audioSource.isPlaying
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
            audioSource.clip = scanLoopSound;
            audioSource.loop = true;

            // เริ่มเสียงที่ 2 จาก Volume 0
            audioSource.volume = 0f;

            audioSource.Play();

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

                audioSource.volume = Mathf.Lerp(
                    0f,
                    loopVolume,
                    t
                );

                yield return null;
            }

            audioSource.volume = loopVolume;
        }
    }

    // =====================================
    // หยุดเสียง
    // =====================================
    void StopScanAudio()
    {
        if (audioCoroutine != null)
        {
            StopCoroutine(audioCoroutine);
            audioCoroutine = null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();

            audioSource.clip = null;
            audioSource.loop = false;
            audioSource.volume = 0f;
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