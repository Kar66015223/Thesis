using System.Collections;
using TMPro;
using UnityEngine;

public class FadeOutText : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private float fadeDuration = 2f;

    private Coroutine fadeRoutine;

    void Awake()
    {
        if (text == null)
            text = GetComponent<TMP_Text>();
    }

    public void Show(string message)
    {
        text.text = message;
        SetAlpha(1f);

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = isActiveAndEnabled ? StartCoroutine(FadeOut()) : null;
    }

    private IEnumerator FadeOut()
    {
        for (float elapsed = 0f; elapsed < fadeDuration; elapsed += Time.deltaTime)
        {
            SetAlpha(Mathf.Lerp(1f, 0f, elapsed / fadeDuration));
            yield return null;
        }

        SetAlpha(0f);
        fadeRoutine = null;
    }

    private void SetAlpha(float alpha)
    {
        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }
}
