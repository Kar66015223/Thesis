using UnityEngine;

/// <summary>Keeps a UI image over a world target, clamped to the screen edge when the target is off-screen.</summary>
public class UITargetTracking : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private RectTransform imageUI;
    [SerializeField] private float margin = 50f;

    private Camera cam;

    void Start() => cam = Camera.main;

    void LateUpdate()
    {
        if (target == null || imageUI == null)
            return;

        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
                return;
        }

        imageUI.localPosition = ToClampedScreenOffset(cam.WorldToScreenPoint(target.position));

        // Shown only after the first positioning, so it never flashes at the screen centre.
        if (!imageUI.gameObject.activeSelf)
            imageUI.gameObject.SetActive(true);
    }

    public void SetTarget(Transform newTarget) => target = newTarget;

    /// <summary>Offset from screen centre; targets behind the camera are mirrored and pushed to the edge.</summary>
    private Vector3 ToClampedScreenOffset(Vector3 screenPos)
    {
        bool isBehind = screenPos.z < 0f;
        if (isBehind)
            screenPos *= -1f;

        Vector3 offset = screenPos - new Vector3(Screen.width, Screen.height, 0f) / 2f;
        offset.z = 0f;

        float halfWidth = Screen.width / 2f - margin;
        float halfHeight = Screen.height / 2f - margin;

        bool isOffScreen = isBehind || Mathf.Abs(offset.x) > halfWidth || Mathf.Abs(offset.y) > halfHeight;
        if (!isOffScreen)
            return offset;

        // Scale the direction so it touches the nearest screen edge.
        float scaleX = offset.x != 0f ? halfWidth / Mathf.Abs(offset.x) : float.PositiveInfinity;
        float scaleY = offset.y != 0f ? halfHeight / Mathf.Abs(offset.y) : float.PositiveInfinity;
        float scale = Mathf.Min(scaleX, scaleY);

        return float.IsInfinity(scale) ? Vector3.zero : offset * scale;
    }
}
