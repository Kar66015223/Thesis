using UnityEngine;
using System.Collections;
using TMPro;

public class FadeOutText : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private float fadeDuration = 2f;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    public IEnumerator FadeOut()
    {
        float elapsed = 0f;
        Color c = text.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            text.color = c;
            yield return null;
        }

        c.a = 0f;
        text.color = c;
    }

    public void SetAlphaToFull()
    {
        Color c = text.color;
        c.a = 1f;
        text.color = c;
    }
}
