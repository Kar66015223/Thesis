using UnityEngine;
using UnityEngine.UI;

public class TrackingUIManager : MonoBehaviour
{
    [SerializeField] private GameObject trackingImgPrefab;

    private GameObject activeMarker;
    private Transform activeTarget;

    void OnEnable() => UIEvents.TrackingTargetChanged += ShowTarget;

    void OnDisable()
    {
        UIEvents.TrackingTargetChanged -= ShowTarget;
        Clear();
    }

    private void ShowTarget(Transform target, Sprite icon)
    {
        if (target == activeTarget && activeMarker != null)
            return;

        Clear();
        if (target == null || trackingImgPrefab == null)
            return;

        activeTarget = target;
        activeMarker = Instantiate(trackingImgPrefab, transform);

        if (activeMarker.TryGetComponent(out Image image))
            image.sprite = icon;

        if (activeMarker.TryGetComponent(out UITargetTracking tracking))
            tracking.SetTarget(target);
    }

    private void Clear()
    {
        if (activeMarker != null)
            Destroy(activeMarker);

        activeMarker = null;
        activeTarget = null;
    }
}
