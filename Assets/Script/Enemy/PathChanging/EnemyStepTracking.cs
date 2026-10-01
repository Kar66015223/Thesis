using UnityEngine;

public class EnemyStepTracking
{
    private EnemyController ctrl;
    
    private EnemyMovementPointPair activePair;
    private int activeStepIndex = -1;
    private EnemyMovementStep activeStep;
    private bool stepInterrupted = false;

    private bool isWaiting = false;
    private float waitingTimer = 0f;
    private float waitingDuration = 0f;
    public bool IsWaiting => isWaiting;

    private string speedModName = "Step";

    public void Initialize(EnemyController ctrl) => this.ctrl = ctrl;

    public void StartStep(EnemyMovementPointPair pair, int stepIndex)
    {
        activePair = pair;
        activeStepIndex = stepIndex;
        activeStep = pair.movementSequence[stepIndex];
        stepInterrupted = false;

        ctrl.Movement.SetSpeedModifier(speedModName, activeStep.speedMultiplier);

        if (activeStep.movementType == EnemyMovementType.Idle)
            ctrl.ApplyIdlePoint(activeStep.idlePoint);
        else
            ctrl.ApplyPatrolPoints(activeStep.patrolPath.wayPoints);

        if(ctrl.CurrentState is IdleState || ctrl.CurrentState is PatrolState)
        {
            if (activeStep.movementType == EnemyMovementType.Idle)
                ctrl.ChangeState(new IdleState(ctrl));
            else
                ctrl.ChangeState(new PatrolState(ctrl));
        }
    }

    public void InterruptCurrentStep()
    {
        if (isWaiting)
            return;

        if (activeStepIndex < 0 || activeStep == null)
            return;

        ctrl.Movement.RemoveSpeedModifier(speedModName);
        stepInterrupted = true;
    }

    public void ResumeInterruptedStep()
    {
        if (!stepInterrupted || activeStepIndex < 0 || activeStep == null)
            return;

        ctrl.Movement.SetSpeedModifier(speedModName, activeStep.speedMultiplier);

        if (activeStep.movementType == EnemyMovementType.Idle)
            ctrl.ApplyIdlePoint(activeStep.idlePoint);
        else
            ctrl.ApplyPatrolPoints(activeStep.patrolPath.wayPoints);

        if (ctrl.CurrentState is IdleState || ctrl.CurrentState is PatrolState)
        {
            if (activeStep.movementType == EnemyMovementType.Idle)
                ctrl.ChangeState(new IdleState(ctrl));
            else
                ctrl.ChangeState(new PatrolState(ctrl));
        }

        stepInterrupted = false;
    }

    public void ClearActiveStep()
    {
        activePair = null;
        activeStepIndex = -1;
        activeStep = null;
        stepInterrupted = false;
        ctrl.Movement.RemoveSpeedModifier(speedModName);
    }

    public void StartIdleWait(float duration)
    {
        if (duration <= 0f)
            return;

        isWaiting = true;
        waitingDuration = duration;
        waitingTimer = 0f;
    }

    public void UpdateWaiting()
    {
        if (!isWaiting)
            return;

        waitingTimer += Time.deltaTime;
        if (waitingTimer >= waitingDuration)
        {
            isWaiting = false;
            waitingTimer = 0f;
            waitingDuration = 0f;

            ctrl.Movement.RemoveSpeedModifier(speedModName);
            GameEvent.OnChangeEnemyPath?.Invoke(ctrl);
            Debug.Log($"{ctrl.name} fired OnChangeEnemyPath");
        }
    }

    public void CancelWaiting()
    {
        isWaiting = false;
        waitingTimer = 0f;
        waitingDuration = 0f;
    }

    public float GetActiveStepWaitTime()
    {
        if (activeStepIndex < 0 || activeStep == null)
            return 0f;

        return activeStep.toNextStepWaitTime;
    }
}