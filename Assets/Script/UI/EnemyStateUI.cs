using UnityEngine;
using UnityEngine.UI;

public class EnemyStateUI : MonoBehaviour
{
    [SerializeField] private Image mainImg;
    [SerializeField] private Image subImg;

    public void SetIcons(Sprite main, Sprite sub, Color color)
    {
        if (mainImg != null)
        {
            mainImg.sprite = main;
            mainImg.color = color;
        }

        if (subImg == null)
            return;

        subImg.enabled = sub != null;
        subImg.sprite = sub;
    }
}
