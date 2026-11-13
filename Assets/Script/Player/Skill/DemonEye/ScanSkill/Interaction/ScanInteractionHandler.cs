using UnityEngine;

[System.Serializable]
public class ScanInteractionHandler
{
    private GameObject interactor;
    private ScanInteractionDetector detector;
    private IScanInteractable holdTarget;
    private float holdTimer;

    public float Progress =>
        holdTarget == null ?
        0f : holdTarget.HoldTime <= 0f ?
        1f : Mathf.Clamp01(holdTimer / holdTarget.HoldTime);

    public void Initialize(GameObject interactor, ScanInteractionDetector detector)
    {
        this.interactor = interactor;
        this.detector = detector;
    }

    public void BeginHold()
    {
        IScanInteractable target = detector.Current;
        if (target == null || !target.CanInteract())
            return;

        holdTarget = target;
        holdTimer = 0f;
    }

    public void CancelHold()
    {
        holdTarget = null;
        holdTimer = 0f;
    }

    public bool Tick(float deltaTime)
    {
        if (holdTarget == null)
            return false;

        if (detector.Current != holdTarget || !holdTarget.CanInteract())
        {
            CancelHold();
            return false;
        }

        holdTimer += deltaTime;
        if (holdTimer < holdTarget.HoldTime)
            return false;

        IScanInteractable target = holdTarget;
        CancelHold();
        target.Interact(interactor);
        return true;
    }
    
    public bool TryInteract()
    {
        IScanInteractable target = detector.Current;
        if (target == null || !target.CanInteract())
            return false;

        target.Interact(interactor);
        return true;
    }
}