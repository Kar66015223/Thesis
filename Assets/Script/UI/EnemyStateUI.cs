using UnityEngine;
using UnityEngine.UI;

/// <summary>View for one enemy indicator: a main icon (fillable) and an optional small sub icon.</summary>
public class EnemyStateUI : MonoBehaviour
{
    [SerializeField] private Image mainImg;
    [SerializeField] private Image subImg;

    public void SetIcons(Sprite main, Sprite sub)
    {
        if (mainImg != null)
            mainImg.sprite = main;

        if (subImg == null)
            return;

        subImg.enabled = sub != null;
        subImg.sprite = sub;
    }

    public void SetFill(float amount)
    {
        if (mainImg != null)
            mainImg.fillAmount = amount;
    }
}
