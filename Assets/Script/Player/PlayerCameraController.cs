using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Rendering;

/// <summary>Everything the camera needs to know about the player for one frame.</summary>
public readonly struct CameraViewState
{
    public readonly bool IsRunning;
    public readonly bool IsCrouching;
    public readonly bool IsSkillVisionActive;
    public readonly bool IsTired;

    public CameraViewState(bool isRunning, bool isCrouching, bool isSkillVisionActive, bool isTired)
    {
        IsRunning = isRunning;
        IsCrouching = isCrouching;
        IsSkillVisionActive = isSkillVisionActive;
        IsTired = isTired;
    }
}

/// <summary>
/// Third-person camera rig: follow target height, FOV and screen-space volumes.
/// It is told the player's state each frame instead of having its public fields poked from outside.
/// Field names are kept identical to the old version so Inspector values survive.
/// </summary>
[System.Serializable]
public class PlayerCameraController
{
    [Header("Camera")]
    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private CinemachineInputAxisController cameraInput;

    [Header("FOV")]
    [SerializeField] private float defaultFOV = 90f;
    [SerializeField] private float FOVChangeSpeed = 10f;
    [SerializeField] private float runFOVChange = 100f;
    [SerializeField] private float crouchFOVChange = 80f;
    [SerializeField] private float scanSkillFOVChange = 60f;

    [Header("Camera Target Positioning")]
    [SerializeField] private Transform camTarget;
    [SerializeField] private float shoulderOffset = 0.5f;
    [SerializeField] private float targetHeight = 1.3f;
    [SerializeField] private float standCamHeight = 1.3f;
    [SerializeField] private float crouchCamHeight = 1f;
    [SerializeField] private float heightTransitionSpeed = 10f;

    [Header("Camera VFX")]
    [SerializeField] private Volume grayScreenVol;
    [SerializeField] private float volumeFadeSpeed = 10f;
    [SerializeField] private Volume blackVignetteVol;
    [SerializeField, Range(0f, 1f)] private float tiredVignetteWeight = 0.4f;

    private const float VolumeSnapThreshold = 0.01f;

    private Transform followTarget;
    private float currentCamHeight;

    public void Initialize(Transform followTarget)
    {
        this.followTarget = followTarget;

        if (cam == null)
            cam = Object.FindAnyObjectByType<CinemachineCamera>();

        currentCamHeight = standCamHeight;
        targetHeight = standCamHeight;
    }

    /// <summary>Camera-relative movement axes flattened onto the ground plane.</summary>
    public void GetPlanarBasis(out Vector3 forward, out Vector3 right)
    {
        Transform reference = cam != null ? cam.transform : followTarget;

        forward = Vector3.ProjectOnPlane(reference.forward, Vector3.up).normalized;
        right = Vector3.ProjectOnPlane(reference.right, Vector3.up).normalized;
    }

    public void Tick(CameraViewState state)
    {
        targetHeight = state.IsCrouching ? crouchCamHeight : standCamHeight;

        UpdateCameraTarget();
        UpdateFOV(state);
        BlendVolume(grayScreenVol, state.IsSkillVisionActive ? 1f : 0f);
        BlendVolume(blackVignetteVol, state.IsTired ? tiredVignetteWeight : 0f);

        if (cameraInput != null)
            cameraInput.enabled = Cursor.lockState == CursorLockMode.Locked;
    }

    private void UpdateCameraTarget()
    {
        if (camTarget == null || cam == null)
            return;

        Vector3 right = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;

        currentCamHeight = Mathf.Lerp(currentCamHeight, targetHeight, heightTransitionSpeed * Time.deltaTime);
        camTarget.position = followTarget.position + Vector3.up * currentCamHeight + right * shoulderOffset;
    }

    private void UpdateFOV(CameraViewState state)
    {
        if (cam == null)
            return;

        float targetFOV = GetTargetFOV(state);
        cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, targetFOV, FOVChangeSpeed * Time.deltaTime);
    }

    private float GetTargetFOV(CameraViewState state)
    {
        if (state.IsSkillVisionActive) return scanSkillFOVChange;
        if (state.IsRunning) return runFOVChange;
        if (state.IsCrouching) return crouchFOVChange;
        return defaultFOV;
    }

    private void BlendVolume(Volume volume, float targetWeight)
    {
        if (volume == null)
            return;

        volume.weight = Mathf.Lerp(volume.weight, targetWeight, volumeFadeSpeed * Time.deltaTime);

        if (Mathf.Abs(volume.weight - targetWeight) < VolumeSnapThreshold)
            volume.weight = targetWeight;
    }
}
