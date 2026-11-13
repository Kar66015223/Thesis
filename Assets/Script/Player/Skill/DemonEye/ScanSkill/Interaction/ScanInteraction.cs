using UnityEngine;

public class ScanInteraction : MonoBehaviour
{
    [SerializeField] private ScanInteractionDetector detector = new();
    [SerializeField] private ScanInteractionHandler handler = new();

    [Tooltip("Left empty, the ScannerSkill and ScanSkillUI on this object and its children are used.")]
    private ScannerSkill scanSkill;
    private ScanSkillUI scanUI;

    public IScanInteractable Current => detector.Current;

    private PlayerInputHandler inputHandler;

    void Awake()
    {
        if (scanSkill == null)
            scanSkill = GetComponentInParent<ScannerSkill>();
        if (scanUI == null)
            scanUI = GetComponentInChildren<ScanSkillUI>(true);
        inputHandler = GetComponentInParent<PlayerInputHandler>();

        detector.Initialize(inputHandler.transform);
        handler.Initialize(inputHandler.gameObject, detector);
    }

    void OnEnable()
    {
        if (inputHandler != null)
            inputHandler.OnInteractFHeld += HandleInteractHeld;
    }

    void OnDisable()
    {
        if(inputHandler != null)
            inputHandler.OnInteractFHeld -= HandleInteractHeld;

        detector.Clear();

        if (scanUI != null)
            scanUI.ClearCrosshairTarget();
    }

    void LateUpdate()
    {
        if (scanSkill == null || !scanSkill.IsActive)
        {
            detector.Clear();
            handler.CancelHold();
            if (scanUI != null)
                scanUI.ClearCrosshairTarget();
            return;
        }

        detector.CheckInteractable();
        handler.Tick(Time.deltaTime);

        if (scanUI == null)
            return;

        if (detector.HasTarget)
            scanUI.SetCrosshairTarget(detector.Camera, detector.TargetCenter);
        else
            scanUI.ClearCrosshairTarget();

        scanUI.UpdateInteractProgress(handler.Progress);
    }

    private void HandleInteractHeld(bool isHeld)
    {
        if (isHeld && scanSkill != null && scanSkill.IsActive)
            handler.BeginHold();
        else
            handler.CancelHold();
    }
}
