using UnityEngine;

public class Tackle : Skill
{
    public Tackle()
    {
        skillName = "Tackle";
        skillDesc = "Dash and tackle targeted enemy, push them back and stun them briefly";
        type = SkillType.Physical;
        usage = SkillUsage.Press;
        coolDown = 10f;
        cost = 30f;
    }

    public override void Use()
    {
        Debug.Log("Used tackle skill");
    }

    public override void HoldUse(bool isHolding) {}
    public override void HandleUse() {}
}