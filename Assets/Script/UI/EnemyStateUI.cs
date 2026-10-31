using UnityEngine;
using UnityEngine.UI;

public class EnemyStateUI : MonoBehaviour
{
    private static readonly Vector2 ArrowArtDirection = Vector2.up;

    [SerializeField] private Image mainImg;
    [SerializeField] private Image indicatorArrow;

    private RectTransform arrowRect;
    private Vector2 arrowRestPosition;

    void Awake()
    {
        if (indicatorArrow == null)
            return;

        arrowRect = indicatorArrow.rectTransform;
        arrowRestPosition = arrowRect.anchoredPosition;
    }

    public void SetIcons(Sprite main, Color color)
    {
        if (mainImg != null)
        {
            mainImg.sprite = main;
            mainImg.color = color;
            indicatorArrow.color = color;
        }
    }

    public void PointArrow(Vector2 direction)
    {
        if (arrowRect == null)
            return;

        Quaternion rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(ArrowArtDirection, direction));
        arrowRect.localRotation = rotation;
        arrowRect.anchoredPosition = rotation * arrowRestPosition;
    }
}
