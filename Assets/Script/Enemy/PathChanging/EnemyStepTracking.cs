using System;

public class EnemyStepTracking
{
    private const string SpeedModifierKey = "Step";

    private EnemyController ctrl;
    private EnemyMovementStep activeStep;
    private bool stepInterrupted;

    private float waitTimer;
    private float waitDuration;

    public bool IsWaiting { get; private set; }
    public float ActiveStepWaitTime => activeStep != null ? activeStep.toNextStepWaitTime : 0f;

    public event Action<EnemyController> StepWaitCompleted;

    public void Initialize(EnemyController ctrl) => this.ctrl = ctrl;

    public void StartStep(EnemyMovementStep step)
    {
        CancelWaiting();
        activeStep = step;
        stepInterrupted = false;
        ApplyActiveStep();
    }

    public void InterruptCurrentStep()
    {
        if (IsWaiting || activeStep == null)
            return;

        ctrl.Movement.RemoveSpeedModifier(SpeedModifierKey);
        stepInterrupted = true;
    }

    public void ResumeInterruptedStep()
    {
        if (!stepInterrupted || activeStep == null)
            return;

        ApplyActiveStep();
        stepInterrupted = false;
    }

    public void ClearActiveStep()
    {
        activeStep = null;
        stepInterrupted = false;
        ctrl.Movement.RemoveSpeedModifier(SpeedModifierKey);
    }

    public void StartIdleWait(float duration)
    {
        if (duration <= 0f)
            return;

        IsWaiting = true;
        waitDuration = duration;
        waitTimer = 0f;
    }

    public void UpdateWaiting()
    {
        if (!IsWaiting)
            return;

        waitTimer += UnityEngine.Time.deltaTime;
        if (waitTimer < waitDuration)
            return;

        CancelWaiting();
        ctrl.Movement.RemoveSpeedModifier(SpeedModifierKey);
        StepWaitCompleted?.Invoke(ctrl);
    }
    
    public void CancelWaiting()
    {
        IsWaiting = false;
        waitTimer = 0f;
        waitDuration = 0f;
    }

    private void ApplyActiveStep()
    {
        ctrl.Movement.SetSpeedModifier(SpeedModifierKey, activeStep.speedMultiplier);

        if (activeStep.movementType == EnemyMovementType.Idle)
            ctrl.ApplyIdlePoint(activeStep.idlePoint);
        else
            ctrl.ApplyPatrolPoints(activeStep.patrolPath.points);

        if (ctrl.IsInDefaultState)
            ctrl.ReturnToDefaultState();
    }
}