using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerFlashbangReceiver : MonoBehaviour
{
    [Header("Volume Reference")]
    public Volume globalVolume;

    [Header("Max Flash Settings (โดนจ่อๆ)")]
    public float maxExposure = 8f;
    public float maxHoldTime = 1.5f;
    public float maxFadeTime = 4.0f;

    private ColorAdjustments colorAdjustments;
    private Coroutine flashRoutine;

    void Start()
    {
        if (globalVolume != null && globalVolume.profile.TryGet(out colorAdjustments))
        {
            colorAdjustments.postExposure.value = 0f;
        }
    }

    // intensity: ค่า 0.0 (ไกลสุดขอบ) ถึง 1.0 (โดนจ่อหน้า)
    public void ApplyFlash(float intensity)
    {
        if (colorAdjustments == null) return;

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashSequence(intensity));
    }

    private IEnumerator FlashSequence(float intensity)
    {
        // ยิ่งโดนใกล้ แสงยิ่งจ้า ค้างนาน และเฟดช้า
        float targetExposure = maxExposure * intensity;
        float holdDuration = maxHoldTime * intensity;
        float fadeDuration = maxFadeTime * intensity;

        // 1. ระเบิดแสงขาวทันที
        colorAdjustments.postExposure.value = targetExposure;

        // 2. ค้างความขาวไว้
        yield return new WaitForSeconds(holdDuration);

        // 3. ค่อยๆ เฟดกลับเป็นปกติ
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            colorAdjustments.postExposure.value = Mathf.Lerp(targetExposure, 0f, elapsed / fadeDuration);
            yield return null;
        }

        colorAdjustments.postExposure.value = 0f;
    }
}