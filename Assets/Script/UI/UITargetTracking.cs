using UnityEngine;

public class UITargetTracking : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private RectTransform imageUI;
    [SerializeField] private float margin = 50f;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (target == null)
            return;

        // Vector3 screenPos = cam.WorldToScreenPoint(target.position);

        // bool isOffScreen =
        //     screenPos.z < 0 ||
        //     screenPos.x <= margin ||
        //     screenPos.x >= Screen.width - margin ||
        //     screenPos.y <= margin ||
        //     screenPos.y >= Screen.height - margin;

        imageUI.gameObject.SetActive(true);

        Vector3 screenPos = cam.WorldToScreenPoint(target.position);
        PositionImage(screenPos);
    }

    private void PositionImage(Vector3 screenPos)
    {
        // flip position if behind
        bool isBehind = screenPos.z < 0;
        if (isBehind)
            screenPos *= -1;

        Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2;
        screenPos -= screenCenter;

        // Rotate to point the top of image to the position
        // float angle = Mathf.Atan2(screenPos.y, screenPos.x) * Mathf.Rad2Deg;
        // arrowUI.rotation = Quaternion.Euler(0, 0, angle - 90);

        float screenX = Screen.width / 2 - margin;
        float screenY = Screen.height / 2 - margin;

        if (isBehind || Mathf.Abs(screenPos.x) > screenX || Mathf.Abs(screenPos.y) > screenY)
        {
            float m = screenPos.y / screenPos.x;

            if (Mathf.Abs(screenPos.x) > Mathf.Abs(screenPos.y / (screenY / screenX)))
            {
                // Clamp right & left
                screenPos = new Vector3(
                    screenPos.x > 0 ? screenX : -screenX, (screenPos.x > 0 ? screenX : -screenX) * m, 0);
            }
            else
            {
                // Clamp top & bottom
                screenPos = new Vector3(
                    (screenPos.y > 0 ? screenY : -screenY) / m, screenPos.y > 0 ? screenY : -screenY, 0);
            }
        }

        imageUI.localPosition = new(screenPos.x, screenPos.y, 0);
    }

    public void SetTarget(Transform target) => this.target = target;
}
