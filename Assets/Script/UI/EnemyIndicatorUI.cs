using UnityEngine;

public class EnemyIndicatorUI : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private RectTransform imageUI;
    [Tooltip("Distance the indicator always sits from the screen centre, in canvas units (1920x1080 reference).")]
    [SerializeField, Min(0f)] private float radius = 400f;

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

        imageUI.localPosition = GetDirectionFromCentre(cam.WorldToScreenPoint(target.position)) * radius;

        if (!imageUI.gameObject.activeSelf)
            imageUI.gameObject.SetActive(true);
    }

    public void SetTarget(Transform newTarget) => target = newTarget;

    private Vector3 GetDirectionFromCentre(Vector3 screenPos)
    {
        Vector3 offset = screenPos - new Vector3(Screen.width, Screen.height, 0f) / 2f;
        offset.z = 0f;

        bool isBehind = screenPos.z < 0f;
        if (isBehind)
            offset = -offset;

        return offset != Vector3.zero ? offset.normalized : Vector3.down;
    }
}
