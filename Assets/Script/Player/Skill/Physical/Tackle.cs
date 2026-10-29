using UnityEngine;

public class Tackle : Skill
{
    private const int MaxCandidates = 8;
    private const float MaxDashTimeMultiplier = 2f;
    private const float HitTolerance = 0.05f;

    private readonly TackleData data;
    private readonly Collider[] overlapBuffer = new Collider[MaxCandidates];

    private Component trackedTarget;
    private Component dashTarget;
    private bool isDashing;
    private float dashElapsed;

    public Tackle(TackleData data, SkillContext context) : base(data, context) => this.data = data;

    public override void OnInputPressed()
    {
        if (isDashing || trackedTarget == null || Context.Body == null || Context.Body.IsHiding)
            return;

        dashTarget = trackedTarget;
        isDashing = true;
        dashElapsed = 0f;
        Context.Body.SetCanMove(false);
        SetCatchImmune(true);
    }

    public override void Tick(float deltaTime)
    {
        UpdateTrackedTarget();

        if (isDashing)
            UpdateDash(deltaTime);
    }

    private void UpdateDash(float deltaTime)
    {
        if (dashTarget == null)
        {
            EndDash(hit: false);
            return;
        }

        Vector3 toTarget = FlatOffsetToTarget();
        float remaining = toTarget.magnitude - data.hitDistance;
        if (remaining > 0f)
        {
            Vector3 direction = toTarget.normalized;
            float speed = data.dashRange / data.dashDuration;

            Context.Body.FaceDirection(direction);
            Context.Body.Dash(direction * Mathf.Min(speed * deltaTime, remaining));
        }

        if (FlatOffsetToTarget().magnitude <= data.hitDistance + HitTolerance)
        {
            EndDash(hit: true);
            return;
        }

        dashElapsed += deltaTime;
        if (dashElapsed >= data.dashDuration * MaxDashTimeMultiplier)
            EndDash(hit: false);
    }

    private void EndDash(bool hit)
    {
        isDashing = false;
        Context.Body.SetCanMove(true);

        if (hit && dashTarget is IStunnable stunnable)
            stunnable.Stun();

        SetCatchImmune(false);
        dashTarget = null;
    }

    private Vector3 FlatOffsetToTarget()
    {
        Vector3 offset = dashTarget.transform.position - Context.Body.Position;
        offset.y = 0f;

        return offset;
    }

    private void SetCatchImmune(bool isImmune)
    {
        if (Context.Struggle != null)
            Context.Struggle.SetCatchImmune(isImmune);
    }

    private void UpdateTrackedTarget()
    {
        Component nearest = FindNearestStunnable();
        if (nearest == trackedTarget)
            return;

        trackedTarget = nearest;
        UIEvents.SetTrackingTarget(trackedTarget != null ? trackedTarget.transform : null, data.usePromptIcon);
    }

    private Component FindNearestStunnable()
    {
        if (Context.Body == null)
            return null;

        Vector3 origin = Context.Body.Position;
        int count = Physics.OverlapSphereNonAlloc(origin, data.dashRange, overlapBuffer, data.enemyLayer);

        Component nearest = null;
        float nearestSqr = float.PositiveInfinity;

        for (int i = 0; i < count; i++)
        {
            IStunnable stunnable = overlapBuffer[i].GetComponentInParent<IStunnable>();
            if (!(stunnable is Component component))
                continue;

            float sqr = (component.transform.position - origin).sqrMagnitude;
            if (sqr < nearestSqr)
            {
                nearestSqr = sqr;
                nearest = component;
            }
        }

        return nearest;
    }
}
