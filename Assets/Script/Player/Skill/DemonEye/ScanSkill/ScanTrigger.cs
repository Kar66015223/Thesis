using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanTrigger : MonoBehaviour
{
    private static readonly List<IScannable> componentBuffer = new();
    private readonly HashSet<IScannable> scanned = new();

    [SerializeField] private float maxRadius = 50f;
    [SerializeField] private float expandDuration = 3f;
    public AnimationCurve growthCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    private SphereCollider col;

    void Awake()
    {
        col = GetComponent<SphereCollider>();
    }

    void OnEnable()
    {
        StartCoroutine(Expand());
    }

    private IEnumerator Expand()
    {
        col.radius = 0f;

        for (float elapsed = 0f; elapsed < expandDuration; elapsed += Time.deltaTime)
        {
            float t = growthCurve.Evaluate(elapsed / expandDuration);
            col.radius = Mathf.Lerp(0f, maxRadius, t);
            yield return null;
        }

        col.radius = maxRadius;
    }

    void OnTriggerEnter(Collider other)
    {
        other.GetComponents(componentBuffer);
        foreach (IScannable scannable in componentBuffer)
        {
            if (scanned.Add(scannable))
                scannable.OnScanEnter();
        }
    }

    void OnTriggerExit(Collider other)
    {
        other.GetComponents(componentBuffer);
        foreach (IScannable scannable in componentBuffer)
        {
            if (scanned.Remove(scannable))
                scannable.OnScanExit();
        }
    }

    void OnDisable()
    {
        StopAllCoroutines();
        foreach (IScannable scannable in scanned)
        {
            if (scannable is Object unityObj && unityObj != null)
                scannable.OnScanExit();
        }

        scanned.Clear();
    }
}
