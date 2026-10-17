using UnityEngine;

/// <summary>
/// Dashes to the enemy that was targeted when the dash started and stuns it on contact.
/// The user can't be caught mid-dash, so tackling a chasing enemy no longer ends in a grab.
/// Talks to enemies only through <see cref="IStunnable"/>, so it no longer creates enemy states itself.
/// </summary>
public class Tackle : Skill
{
    private const int MaxCandidates = 8;
    // Give up when the target can't be reached in time (it outran the dash, or a wall is in the way).
    private const float MaxDashTimeMultiplier = 2f;
    private const float HitTolerance = 0.05f;

    private readonly TackleData data;
    private readonly Collider[] overlapBuffer = new Collider[MaxCandidates];

    private Component trackedTarget;   // nearest stunnable in range (shown by the tracking UI)
    private Component dashTarget;      // locked when the dash starts
    private bool isDashing;
    private float dashElapsed;

    public Tackle(TackleData data, SkillContext context) : base(data, context) => this.data = data;

    public override void OnInputPressed()
    {
        if (isDashing || trackedTarget == null || Context.Body == null || Context.Body.IsHiding)
            return;

        // Lock the target now: losing sight of it mid-dash used to leave the player frozen forever.
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
        // The target was destroyed mid-dash.
        if (dashTarget == null)
        {
            EndDash(hit: false);
            return;
        }

        // Steer every frame so a moving target is still reached, and stop at hit distance instead of pushing into it.
        Vector3 toTarget = FlatOffsetToTarget();
        float remaining = toTarget.magnitude - data.hitDistance;
        if (remaining > 0f)
        {
            Vector3 direction = toTarget.normalized;
            float speed = data.dashRange / data.dashDuration;

            Context.Body.FaceDirection(direction);
            Context.Body.Dash(direction * Mathf.Min(speed * deltaTime, remaining));
        }

        // Measure after moving: collisions may have stopped the dash short of the target.
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

        // Stun before the user becomes catchable again so the target can't grab them on the same frame.
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
