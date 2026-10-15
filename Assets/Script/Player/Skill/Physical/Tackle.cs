using UnityEngine;

/// <summary>
/// Dashes forward and stuns the enemy that was targeted when the dash started.
/// Talks to enemies only through <see cref="IStunnable"/>, so it no longer creates enemy states itself.
/// </summary>
public class Tackle : Skill
{
    private const int MaxCandidates = 8;

    private readonly TackleData data;
    private readonly Collider[] overlapBuffer = new Collider[MaxCandidates];

    private Component trackedTarget;   // nearest stunnable in range (shown by the tracking UI)
    private Component dashTarget;      // locked when the dash starts
    private bool isDashing;
    private float dashElapsed;

    public Tackle(TackleData data, SkillContext context) : base(data, context) => this.data = data;

    public override void OnInputPressed()
    {
        if (isDashing || trackedTarget == null || Context.Body == null)
            return;

        // Lock the target now: losing sight of it mid-dash used to leave the player frozen forever.
        dashTarget = trackedTarget;
        isDashing = true;
        dashElapsed = 0f;
        Context.Body.SetCanMove(false);
    }

    public override void Tick(float deltaTime)
    {
        UpdateTrackedTarget();

        if (isDashing)
            UpdateDash(deltaTime);
    }

    private void UpdateDash(float deltaTime)
    {
        dashElapsed += deltaTime;

        float speed = data.dashRange / data.dashDuration;
        Context.Body.Dash(deltaTime * speed * Context.Body.Forward);

        if (dashElapsed < data.dashDuration)
            return;

        isDashing = false;
        Context.Body.SetCanMove(true);

        if (dashTarget != null && dashTarget is IStunnable stunnable)
            stunnable.Stun();

        dashTarget = null;
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
