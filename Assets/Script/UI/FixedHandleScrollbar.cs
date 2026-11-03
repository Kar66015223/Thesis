using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Scrollbar))]
public class FixedHandleScrollbar : MonoBehaviour
{
    [Tooltip("Scroll view this scrollbar moves. Leave it out of the ScrollRect's own Vertical Scrollbar field, " +
             "which would resize the handle to the visible share of the content.")]
    [SerializeField] private ScrollRect scrollRect;

    private Scrollbar scrollbar;

    void Awake() => scrollbar = GetComponent<Scrollbar>();

    void OnEnable()
    {
        if (scrollRect == null)
            return;

        scrollRect.onValueChanged.AddListener(HandleScrolled);
        scrollbar.onValueChanged.AddListener(HandleScrollbarMoved);
        scrollbar.SetValueWithoutNotify(scrollRect.verticalNormalizedPosition);
    }

    void OnDisable()
    {
        if (scrollRect == null)
            return;

        scrollRect.onValueChanged.RemoveListener(HandleScrolled);
        scrollbar.onValueChanged.RemoveListener(HandleScrollbarMoved);
    }

    private void HandleScrolled(Vector2 position) => scrollbar.SetValueWithoutNotify(position.y);

    private void HandleScrollbarMoved(float value) => scrollRect.verticalNormalizedPosition = value;
}
