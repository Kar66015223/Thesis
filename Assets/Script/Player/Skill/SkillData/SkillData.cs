using UnityEngine;

public abstract class SkillData : ScriptableObject
{
    public string skillName;
    public string skillDesc;
    public Sprite icon;
    public SkillType type;
    public SkillUsage usage;
    public float cooldown;
    public float cost;
}