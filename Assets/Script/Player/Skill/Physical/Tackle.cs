using System.Collections;
using UnityEngine;

public class Tackle : Skill
{
    private float dashRange;
    private float dashDuration;
    private bool isDashing = false;

    public Tackle(SkillContext context, float dashRange, float dashDuration)
    {
        skillName = "Tackle";
        skillDesc = "Dash and tackle targeted enemy, push them back and stun them briefly";
        type = SkillType.Physical;
        usage = SkillUsage.Press;
        coolDown = 10f;
        cost = 30f;
        this.context = context;

        this.dashRange = dashRange;
        this.dashDuration = dashDuration;
    }

    public override void Use()
    {
        Debug.Log("Used tackle skill");
    }

    private IEnumerator DashRoutine(GameObject user)
    {
        isDashing = true;

        if (user.TryGetComponent(out CharacterController ctrl))
            ctrl.enabled = false;

        float elapsedTime = 0f;
        while (elapsedTime < dashDuration)
        {
            float normalizedTime = elapsedTime / dashDuration;
            user.transform.Translate(Vector3.forward * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (user.TryGetComponent(out CharacterController _))
            ctrl.enabled = false;

        isDashing = false;
    }

    public override void HoldUse(bool isHolding) {}
    public override void HandleUse() {}
}