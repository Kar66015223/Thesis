[System.Serializable]
public class ScanInteractionHandler
{
    private ScanInteractionDetector detector;

    public void Initialize(ScanInteractionDetector detector) => this.detector = detector;
    
    public bool TryInteract()
    {
        IScanInteractable target = detector.Current;
        if (target == null || !target.CanInteract())
            return false;

        target.Interact();
        return true;
    }
}