using UnityEngine;

public class ScanSkillUI : MonoBehaviour
{
    [SerializeField] private GameObject rootPanel;
    [Tooltip("Left empty, the child of Root Panel named Crosshair is used.")]
    [SerializeField] private RectTransform crosshair;
    [Tooltip("How fast the crosshair slides to a scan interactable and back to the centre. 0 = snap instantly.")]
    [SerializeField, Min(0f)] private float crosshairMoveSpeed = 20f;

    private ScannerSkill scanSkill;
    private Canvas canvas;
    private RectTransform crosshairParent;
    private Vector2 defaultCrosshairPosition;
    private Vector2 targetCrosshairPosition;

    void Awake()
    {
        scanSkill = GetComponentInParent<ScannerSkill>();
        canvas = GetComponentInParent<Canvas>();

        if (crosshair == null)
            return;

        crosshairParent = crosshair.parent as RectTransform;
        defaultCrosshairPosition = crosshair.anchoredPosition;
        targetCrosshairPosition = defaultCrosshairPosition;
    }

    void Update() => ToggleRootPanel(scanSkill.IsActive);

    void LateUpdate()
    {
        if (crosshair == null)
            return;

        if (!rootPanel.activeSelf || crosshairMoveSpeed <= 0f)
        {
            crosshair.anchoredPosition = targetCrosshairPosition;
            return;
        }

        crosshair.anchoredPosition = Vector2.Lerp(
            crosshair.anchoredPosition, targetCrosshairPosition, crosshairMoveSpeed * Time.unscaledDeltaTime);
    }

    public void ToggleRootPanel(bool isShow) => rootPanel.SetActive(isShow);

    public void SetCrosshairTarget(Camera cam, Vector3 worldPosition)
    {
        if (crosshair == null || crosshairParent == null || cam == null)
            return;

        Vector3 screenPoint = cam.WorldToScreenPoint(worldPosition);
        if (screenPoint.z <= 0f)
        {
            ClearCrosshairTarget();
            return;
        }

        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(crosshairParent, screenPoint, uiCamera, out Vector2 local))
            return;

        Vector2 anchorCentre = (crosshair.anchorMin + crosshair.anchorMax) * 0.5f;
        Vector2 anchorOffset = Vector2.Scale(anchorCentre - crosshairParent.pivot, crosshairParent.rect.size);
        targetCrosshairPosition = local - anchorOffset;
    }

    public void ClearCrosshairTarget() => targetCrosshairPosition = defaultCrosshairPosition;
}
