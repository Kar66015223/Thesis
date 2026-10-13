using UnityEngine;

public class SkillSlot : MonoBehaviour
{
    private Vector2 normalSize;
    [SerializeField] private Vector2 selectedSize;

    public bool isSelected = false;

    private RectTransform rTransform;

    void Awake()
    {
        rTransform = GetComponent<RectTransform>();
        normalSize = rTransform.sizeDelta;
    }

    void Update()
    {
        if (isSelected)
        {
            rTransform.sizeDelta = selectedSize;
        }
        else
        {
            rTransform.sizeDelta = normalSize;
        }
    }
}