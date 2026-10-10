using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Skills/Tackle")]
public class TackleData : ScriptableObject
{
    public string skillName = "Tackle";
    public string skillDesc = "Dash and tackle targeted enemy, push them back and stun them briefly";
    public float dashRange = 1f;
    public float dashDuration = 1f;
    public float coolDown = 10f;
    public float cost = 30f;
    public SkillType type = SkillType.Physical;
    public SkillUsage usage = SkillUsage.Press;
    public Sprite icon;

    public LayerMask enemyLayer;
}