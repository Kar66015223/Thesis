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

        detector.Initialize(scanSkill != null ? scanSkill.transform : transform);
        handler.Initialize(detector);
    }

    void OnEnable()
    {
        if (inputHandler != null)
            inputHandler.OnInteractFInput += TryInteract;
    }

    void OnDisable()
    {
        if(inputHandler != null)
            inputHandler.OnInteractFInput -= TryInteract;

        detector.Clear();

        if (scanUI != null)
            scanUI.ClearCrosshairTarget();
    }

    void LateUpdate()
    {
        if (scanSkill == null || !scanSkill.IsActive)
        {
            detector.Clear();
            if (scanUI != null)
                scanUI.ClearCrosshairTarget();
            return;
        }

        detector.CheckInteractable();

        if (scanUI == null)
            return;

        if (detector.HasTarget)
            scanUI.SetCrosshairTarget(detector.Camera, detector.TargetCenter);
        else
            scanUI.ClearCrosshairTarget();
    }

    private void TryInteract()
    {
        if (handler.TryInteract())
            Debug.Log("Scan Interaction Successful");
    }
}
