using UnityEngine;
using Unity.Cinemachine;

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

    [Header("Camera Target Positioning")]
    [SerializeField] private Transform camTarget;
    [SerializeField] private float shoulderOffset = 0.5f;
    public float targetHeight = 1.3f;

    [SerializeField] private float standCamHeight = 1.3f;
    [SerializeField] private float crouchCamHeight = 1f;

    [SerializeField] private float heightTransitionSpeed = 10f;
    private float currentCamHeight;

    [Header("Skill")]
    public float scanSkillFOVChange = 60f;

    public void Initialize(PlayerController controller)
    {
        this.controller = controller;
        if (cam == null)
            cam = Object.FindAnyObjectByType<CinemachineCamera>();

        currentCamHeight = standCamHeight;
        targetFOV = defaultFOV;
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

    public void ChangeFOV(bool isChanging, float value)
    {
        targetFOV = isChanging ? value : defaultFOV;
    }

    public void UpdateFOVChange()
    {
        cam.Lens.FieldOfView = Mathf.Lerp(cam.Lens.FieldOfView, targetFOV, FOVChangeSpeed * Time.deltaTime);
    }
}