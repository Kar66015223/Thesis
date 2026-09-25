using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Rendering;

[System.Serializable]
public class PlayerCameraController
{
    private PlayerController controller;

    [Header("Camera")]
    public CinemachineCamera cam;
    [HideInInspector] public Vector3 camForward;
    [HideInInspector] public Vector3 camRight;

    [Header("FOV")]
    [SerializeField] private float defaultFOV = 90f;
    [SerializeField] private float FOVChangeSpeed = 10f;
    private float targetFOV;

    public float runFOVChange = 100f;
    [HideInInspector] public bool isRunning;

    public float crouchFOVChange = 80f;
    [HideInInspector] public bool isCrouching;

    public float scanSkillFOVChange = 60f;
    [HideInInspector] public bool isDemonEyeActive;


    [Header("Camera Target Positioning")]
    [SerializeField] private Transform camTarget;
    [SerializeField] private float shoulderOffset = 0.5f;
    public float targetHeight = 1.3f;

    [SerializeField] private float standCamHeight = 1.3f;
    [SerializeField] private float crouchCamHeight = 1f;

    [SerializeField] private float heightTransitionSpeed = 10f;
    private float currentCamHeight;

    [Header("Camera VFX")]
    [SerializeField] private Volume greyScreenVolume;
    [SerializeField] private float volumeFadeSpeed = 10f;

    private float currentVolumeWeight;

    public void Initialize(PlayerController controller)
    {
        this.controller = controller;
        if (cam == null)
            cam = Object.FindAnyObjectByType<CinemachineCamera>();

        currentCamHeight = standCamHeight;
        targetFOV = defaultFOV;
        currentVolumeWeight = 0f;
    }

    public void Update()
    {
        UpdateCameraTarget();
        UpdateFOVChange();
        UpdateCameraVFX();
    }

    public void UpdateCameraTarget()
    {
        if (camTarget == null || cam == null)
            return;

        Vector3 right = cam.transform.right;
        right.y = 0;
        right.Normalize();

        currentCamHeight = Mathf.Lerp(currentCamHeight, targetHeight, heightTransitionSpeed * Time.deltaTime);
        camTarget.position =
            controller.transform.position + (Vector3.up * currentCamHeight) + (right * shoulderOffset);
    }

    public void ChangeCamHeight(bool IsCrouching)
        => targetHeight = IsCrouching ? crouchCamHeight : standCamHeight;

    public void UpdateFOVChange()
    {
        if (isDemonEyeActive)
            targetFOV = scanSkillFOVChange;
        else if (isRunning)
            targetFOV = runFOVChange;
        else if (isCrouching)
            targetFOV = crouchFOVChange;
        else
            targetFOV = defaultFOV;

        cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, targetFOV, FOVChangeSpeed * Time.deltaTime);
    }
    
    public void UpdateCameraVFX()
    {
        if (isDemonEyeActive)
            currentVolumeWeight = 1f;
        else
            currentVolumeWeight = 0f;

        greyScreenVolume.weight = Mathf.Lerp(
            greyScreenVolume.weight, currentVolumeWeight, volumeFadeSpeed * Time.deltaTime);

        if (Mathf.Abs(greyScreenVolume.weight - currentVolumeWeight) < 0.01f)
            greyScreenVolume.weight = currentVolumeWeight;
    }
}