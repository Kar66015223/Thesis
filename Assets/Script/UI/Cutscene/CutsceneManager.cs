using TMPro;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField] private CutsceneSequence[] sequences;
    private int currentIndex = 0;
    [SerializeField] private TMP_Text cutsceneText;
    [SerializeField] private GameObject nextPrompt;

    private bool canContinue;

    void Start()
    {
        ShowSequence(0);
    }

    void Update()
    {
        if (!cutsceneText.TryGetComponent<TypewriterEffect>(out var effect))
            return;

        canContinue = !effect.IsPlaying;

        if (Input.GetMouseButtonDown(0))
        {
            if (canContinue)
                NextSequence();
        }

        nextPrompt.SetActive(canContinue);
    }

    void ShowSequence(int index)
    {
        for (int i = 0; i < sequences.Length; i++)
        {
            if (i == index)
            {
                if (cutsceneText.TryGetComponent(out TypewriterEffect effect))
                    effect.SetFullText(sequences[i].text);
            }
        }
    }

    void NextSequence()
    {
        if (currentIndex < sequences.Length - 1)
        {
            currentIndex++;
            ShowSequence(currentIndex);
        }
        else
            SceneLoader.LoadScene(SceneConstants.HIDEOUT_NAME);
    }
}