using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class TrackingUIManager : MonoBehaviour
{
    [SerializeField] private GameObject trackingImgPrefab;
    private Dictionary<Transform, GameObject> activeTrackingUIs = new();

    void OnEnable()
    {
        GameEvent.OnShowTrackingUI += CreateOrUpdateTrackingUI;
    }

    void OnDisable()
    {
        GameEvent.OnShowTrackingUI -= CreateOrUpdateTrackingUI;
    }

    private void CreateOrUpdateTrackingUI(Transform target, Sprite sprite)
    {
        if(target == null)
        {
            foreach (var kv in activeTrackingUIs)
                Destroy(kv.Value);

            activeTrackingUIs.Clear();
            return;
        }
        
        GameObject uiObj;

        if (activeTrackingUIs.ContainsKey(target))
            uiObj = activeTrackingUIs[target];
        else
        {
            uiObj = Instantiate(trackingImgPrefab, gameObject.transform);
            activeTrackingUIs.Add(target, uiObj);

            if (uiObj.TryGetComponent(out Image img))
                img.sprite = sprite;

            if (uiObj.TryGetComponent(out UITargetTracking tracking))
                tracking.SetTarget(target);
        }
    }   
}