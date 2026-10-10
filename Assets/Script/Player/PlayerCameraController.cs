using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Rendering;

[System.Serializable]
public class PlayerCameraController
{
    private PlayerController controller;
    private PlayerStamina stamina;

    [Header("Camera")]
    public CinemachineCamera cam;
    [SerializeField] private CinemachineInputAxisController cameraInput;
    // [SerializeField] private CinemachineBasicMultiChannelPerlin perlin;

    [HideInInspector] public Vector3 camForward;
    [HideInInspector] public Vector3 camRight;

    [Header("FOV")]
    [SerializeField] private float defaultFOV = 90f;
    [SerializeField] private float FOVChangeSpeed = 10f;
    private float targetFOV;

    public float runFOVChange = 100f;
    public float crouchFOVChange = 80f;
    public float scanSkillFOVChange = 60f;

    [HideInInspector] public bool isRunning;
    [HideInInspector] public bool isCrouching;
    [HideInInspector] public bool isDemonEyeActive;
    [HideInInspector] public bool isTired;


    [Header("Camera Target Positioning")]
    [SerializeField] private Transform camTarget;
    [SerializeField] private float shoulderOffset = 0.5f;
    public float targetHeight = 1.3f;

    [SerializeField] private float standCamHeight = 1.3f;
    [SerializeField] private float crouchCamHeight = 1f;

    [SerializeField] private float heightTransitionSpeed = 10f;
    private float currentCamHeight;

    [Header("Camera VFX")]
    [SerializeField] private Volume grayScreenVol;
    [SerializeField] private float volumeFadeSpeed = 10f;
    private float curGrayScreenWeight;

    [SerializeField] private Volume blackVignetteVol;
    private float curBlackVignetteWeight;

    public void Initialize(PlayerController controller)
    {
        this.controller = controller;
        if (cam == null)
            cam = Object.FindAnyObjectByType<CinemachineCamera>();

        stamina = controller.GetComponent<PlayerStamina>();

        currentCamHeight = standCamHeight;
        targetFOV = defaultFOV;
        curGrayScreenWeight = 0f;
    }

    public void Update()
    {
        UpdateCameraTarget();
        UpdateFOVChange();
        UpdateCameraVFX();

        cameraInput.enabled = Cursor.lockState == CursorLockMode.Locked;
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
        if (stamina != null)
            isRunning = stamina.IsRunning;

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
            curGrayScreenWeight = 1f;
        else
            curGrayScreenWeight = 0f;

        grayScreenVol.weight = Mathf.Lerp(
            grayScreenVol.weight, curGrayScreenWeight, volumeFadeSpeed * Time.deltaTime);

        if (Mathf.Abs(grayScreenVol.weight - curGrayScreenWeight) < 0.01f)
            grayScreenVol.weight = curGrayScreenWeight;

        if (isTired)
            curBlackVignetteWeight = 0.4f;
        else
            curBlackVignetteWeight = 0f;

        blackVignetteVol.weight = Mathf.Lerp(
            blackVignetteVol.weight, curBlackVignetteWeight, volumeFadeSpeed * Time.deltaTime);

        if (Mathf.Abs(blackVignetteVol.weight - curBlackVignetteWeight) < 0.01f)
            blackVignetteVol.weight = curBlackVignetteWeight;
    }

    // public void ToggleShake(bool isOn)
    // {
    //     perlin.AmplitudeGain = isOn ? 1 : 0;
    // }
}