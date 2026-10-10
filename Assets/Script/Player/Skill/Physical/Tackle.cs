using UnityEngine;

public class Tackle : Skill
{
    private TackleData data;

    private bool isDashing = false;
    private float dashElapsed = 0f;

    public Tackle(TackleData data, SkillContext context)
    {
        this.data = data;
        skillName = "Tackle";
        skillDesc = "Dash and tackle targeted enemy, push them back and stun them briefly";
        type = SkillType.Physical;
        usage = SkillUsage.Press;
        coolDown = 10f;
        cost = 30f;
        this.context = context;
    }

    public override void Use()
    {
        if (isDashing)
            return;
        isDashing = true;
        dashElapsed = 0f;
    }

    public override void Tick(float deltaTime)
    {
        if (!isDashing)
            return;

        dashElapsed += Time.deltaTime;
        float t = dashElapsed / data.dashDuration;
        float speed = data.dashRange / data.dashDuration;

        if (context.User.TryGetComponent(out CharacterController ctrl))
            ctrl.Move(deltaTime * speed * context.User.transform.forward);

        if (dashElapsed >= data.dashDuration)
            isDashing = false;
    }

    public override void HoldUse(bool isHolding) {}
    public override void HandleUse() {}
}