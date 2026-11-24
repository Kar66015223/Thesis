using UnityEngine;


[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData/FoodData")]
public class FoodData : ItemData
{
    public float fillAmount;

    public override void Eat(GameObject owner)
    {
        base.Eat(owner);
    }
}