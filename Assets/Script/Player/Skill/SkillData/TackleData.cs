using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Skills/Tackle")]
public class TackleData : SkillData
{
    [Tooltip("How far away an enemy can be targeted.")]
    public float dashRange = 1f;
    [Tooltip("Seconds to cover the whole dash range; closer targets are reached sooner.")]
    public float dashDuration = 1f;
    [Tooltip("Flat distance between the player and the target's centre that counts as a hit. " +
             "Keep it larger than player radius + enemy radius, or the colliders stop the dash first.")]
    public float hitDistance = 1.2f;
    public LayerMask enemyLayer;
    public Sprite usePromptIcon;
}
