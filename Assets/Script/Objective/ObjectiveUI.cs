using TMPro;
using UnityEngine;
using System.Collections;

public class ObjectiveUI : MonoBehaviour
{
    [SerializeField] private GameObject objectivePanel;
    [SerializeField] private TMP_Text objectiveText;

    [SerializeField] private Vector2 hiddenOffset = new(-600f, 0f);
    [SerializeField, Min(0.01f)] private float slideDuration = 0.4f;
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private RectTransform panelRect;
    private Vector2 shownPos;
    private Coroutine slideRoutine;

    private AudioSource source;
    [SerializeField] private AudioClip objectiveChangeSound;

    void Awake()
    {
        panelRect = objectivePanel.GetComponent<RectTransform>();
        shownPos = panelRect.anchoredPosition;
        source = GetComponent<AudioSource>();
    } 

    void OnEnable()
    {
        UIEvents.ChangeObjectiveRequest += SetObjectiveText;
    }

    void OnDisable()
    {
        UIEvents.ChangeObjectiveRequest -= SetObjectiveText;
    }

    public void SetObjectiveText(ObjectiveData objTive)
    {
        objectiveText.text = objTive.objectiveText;
        objectivePanel.SetActive(true);
        source.PlayOneShot(objectiveChangeSound);

        if (slideRoutine != null)
            StopCoroutine(slideRoutine);
        slideRoutine = StartCoroutine(SlideIn());
    }

    private IEnumerator SlideIn()
    {
        Vector2 from = shownPos + hiddenOffset;
        panelRect.anchoredPosition = from;

        for (float t = 0f; t < slideDuration; t += Time.deltaTime)
        {
            panelRect.anchoredPosition =
                Vector2.LerpUnclamped(from, shownPos, slideCurve.Evaluate(t / slideDuration));
            yield return null;
        }

        panelRect.anchoredPosition = shownPos;
        slideRoutine = null;
    }
}