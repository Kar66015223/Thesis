using UnityEngine;

/// <summary>
/// Default no-op implementations so a state only overrides what it cares about
/// (states used to repeat empty OnHearSound/OnSeenTarget/Exit methods), plus shared helpers.
/// </summary>
public abstract class EnemyStateBase : IEnemyState
{
    protected const float ApproachStoppingDistance = 2f;
    protected readonly EnemyController ctrl;
    protected EnemyStateBase(EnemyController ctrl) => this.ctrl = ctrl;
    public virtual EnemyState Indicator => EnemyState.None;

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }

    public virtual void OnHearSound(SoundSignal sound) { }
    public virtual void OnSeenTarget(Transform target) { }

    protected bool HasReachedDestination(float extraTolerance = 0f)
        => !ctrl.Agent.pathPending && ctrl.Agent.remainingDistance <= ctrl.Agent.stoppingDistance + extraTolerance;

    protected void RotateTowards(Quaternion targetRotation)
    {
        ctrl.transform.rotation = Quaternion.Slerp(
            ctrl.transform.rotation, targetRotation, ctrl.LookAtRotationSpeed * Time.deltaTime);
    }

    protected Vector3 FlatDirectionTo(Vector3 worldPos)
    {
        Vector3 direction = worldPos - ctrl.transform.position;
        direction.y = 0f;

        return direction;
    }

    protected void FaceInstantly(Vector3 worldPos)
    {
        Vector3 direction = FlatDirectionTo(worldPos);
        if (direction != Vector3.zero)
            ctrl.transform.rotation = Quaternion.LookRotation(direction);
    }
}