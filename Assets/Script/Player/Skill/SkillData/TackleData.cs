using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Skills/Tackle")]
public class TackleData : SkillData
{
    public float dashRange = 1f;
    public float dashDuration = 1f;
    public LayerMask enemyLayer;
    public Sprite usePromptIcon;
}