using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SkillSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector2 normalSize;
    [SerializeField] private Vector2 hoverSize = new(200f, 200f);
    private Vector2 targetSize;
    [SerializeField] private float hoverSmooth = 10f;
    public UnityEvent onClick;

    private RectTransform rTransform;

    void Awake()
    {
        rTransform = GetComponent<RectTransform>();
        normalSize = rTransform.sizeDelta;
        targetSize = normalSize;
    }

    void Update()
    {
        rTransform.sizeDelta = Vector2.Lerp(
            rTransform.sizeDelta, targetSize, Time.unscaledDeltaTime * hoverSmooth);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetSize = hoverSize;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetSize = normalSize;
    }
}