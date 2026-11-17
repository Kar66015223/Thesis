using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private float showDelay = 0.1f;
    [SerializeField] private string fullText;

    private TMP_Text textComponent;
    private Coroutine typing;

    public bool IsPlaying { get; private set; }

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        textComponent.useMaxVisibleDescender = false;
    } 

    void Start()
    {
        typing ??= StartCoroutine(ShowText());
    }

    IEnumerator ShowText()
    {
        IsPlaying = true;
        textComponent.text = fullText;
        textComponent.maxVisibleCharacters = 0;
        textComponent.ForceMeshUpdate();

        int totalCharacters = textComponent.textInfo.characterCount;

        for (int i = 1; i <= totalCharacters; i++)
        {
            yield return new WaitForSeconds(showDelay);
            textComponent.maxVisibleCharacters = i;
        }

        IsPlaying = false;
    }

    public void SkipEffect()
    {
        if (typing != null)
            StopCoroutine(typing);
        IsPlaying = false;
        textComponent.maxVisibleCharacters = textComponent.textInfo.characterCount;
    }

    public void SetFullText(string fullText)
    {
        this.fullText = fullText;
        if (typing != null)
            StopCoroutine(typing);
        typing = StartCoroutine(ShowText());
    }
}