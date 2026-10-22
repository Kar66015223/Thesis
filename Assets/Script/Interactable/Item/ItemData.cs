using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [Min(1)] public int maxStack = 1;

    [Header("Economy")]
    [Tooltip("Money for each one sold to the shop NPC. 0 = can't be sold (keys, mission items).")]
    [Min(0)] public int sellPrice = 10;
}
