using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Tackle")]
public class TackleData : ScriptableObject
{
    public string skillName = "Tackle";
    public float dashRange = 1f;
    public float dashDuration = 1f;
    public float coolDown = 10f;
    public float cost = 30f;
    public SkillUsage usage = SkillUsage.Press;
    // add icons, descriptions, etc.
}