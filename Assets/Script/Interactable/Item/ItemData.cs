using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    [TextArea] public string itemDesc = "Test.............................................................................";
    public Sprite icon;
    [Min(1)] public int maxStack = 1;
    [Min(0.1f)] public float weight = 0.1f;

    [Header("Economy")]
    [Tooltip("Money for each one sold to the shop NPC. 0 = can't be sold (keys, mission items).")]
    [Min(0)] public int sellPrice = 0;
}
