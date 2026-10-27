using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Eyes of the enemy: finds visible targets a few times per second and runs the two-step
/// "glimpse -> confirmed" timer, reporting both moments to the controller.
/// </summary>
[RequireComponent(typeof(EnemyController))]
public class EnemyVision : MonoBehaviour
{
    private const int MaxTargetsPerScan = 16;

    [SerializeField] private float viewRadius;
    [Range(0, 360)][SerializeField] private float viewAngle;
    [SerializeField] private float viewHeight;
    [Tooltip("Height of the view cone, centred on the eye. Anything whose collider reaches into this band can be seen, " +
             "so a player crouching right in front of the enemy is still inside it.")]
    [SerializeField, Min(0f)] private float viewVerticalSize = 3f;

    [SerializeField] private float defaultViewRadius = 10f;
    [SerializeField] private float viewRadiusChasing = 30f;
    [SerializeField] private float scanInterval = 0.1f;

    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    private readonly Collider[] overlapBuffer = new Collider[MaxTargetsPerScan];
    private readonly List<Transform> visibleTargets = new();
    private readonly List<Transform> confirmedTargets = new();

    private EnemyController ctrl;
    private Coroutine scanRoutine;
    private float confirmTimer;

    public float ViewRadius => viewRadius;
    public float ViewAngle => viewAngle;
    public float ViewVerticalSize => viewVerticalSize;
    public Vector3 EyePosition => transform.position + Vector3.up * viewHeight;
    public IReadOnlyList<Transform> VisibleTargets => visibleTargets;
    public IReadOnlyList<Transform> ConfirmedTargets => confirmedTargets;

    void Awake() => ctrl = GetComponent<EnemyController>();

    void OnEnable() => EnableVision();

    // Unity stops coroutines on disable; forget the handle so OnEnable can restart scanning.
    void OnDisable() => scanRoutine = null;

    void Update()
    {
        if (visibleTargets.Count > 0)
            UpdateConfirmation(visibleTargets[0]);
        else if (confirmTimer > 0f)
            ResetConfirmation();

        viewRadius = ctrl.IsChasing ? viewRadiusChasing : defaultViewRadius;
    }

    public void EnableVision()
    {
        if (scanRoutine == null)
            scanRoutine = StartCoroutine(ScanLoop());
    }

    /// <summary>Stops scanning and forgets what it saw (e.g. while stunned).</summary>
    public void DisableVision()
    {
        if (scanRoutine != null)
        {
            StopCoroutine(scanRoutine);
            scanRoutine = null;
        }

        visibleTargets.Clear();
        ResetConfirmation();
    }

    public Vector3 DirFromAngle(float angleInDegree, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
            angleInDegree += transform.eulerAngles.y;

        return new(Mathf.Sin(angleInDegree * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegree * Mathf.Deg2Rad));
    }

    private void UpdateConfirmation(Transform target)
    {
        // First glimpse...
        if (confirmTimer == 0f)
            ctrl.OnSeenTarget(target);

        confirmTimer += Time.deltaTime;
        float progress = ctrl.ConfirmWaitTime > 0f ? Mathf.Clamp01(confirmTimer / ctrl.ConfirmWaitTime) : 1f;
        EnemyEvents.RaiseConfirmProgressChanged(transform, progress);

        // ...then confirmed after looking long enough.
        if (confirmTimer >= ctrl.ConfirmWaitTime && !confirmedTargets.Contains(target))
        {
            confirmedTargets.Add(target);
            ctrl.OnSeenTarget(target);
        }
    }

    private void ResetConfirmation()
    {
        confirmTimer = 0f;
        confirmedTargets.Clear();
        EnemyEvents.RaiseConfirmProgressChanged(transform, 0f);
    }

    private IEnumerator ScanLoop()
    {
        WaitForSeconds wait = new(scanInterval);
        while (true)
        {
            yield return wait;
            FindVisibleTargets();
        }
    }

    private void FindVisibleTargets()
    {
        visibleTargets.Clear();

        Vector3 eye = EyePosition;
        float bandBottom = eye.y - viewVerticalSize / 2f;
        float bandTop = eye.y + viewVerticalSize / 2f;

        Vector3 forward = transform.forward;
        forward.y = 0f;

        int count = Physics.OverlapSphereNonAlloc(eye, viewRadius, overlapBuffer, targetMask);

        for(int i = 0; i < count; i++)
        {
            Collider col = overlapBuffer[i];
            Bounds bounds = col.bounds;

            if (bounds.max.y < bandBottom || bounds.min.y > bandTop)
                continue;

            // Aim at the highest part of the target that's inside the band.
            Vector3 aimPoint = bounds.center;
            aimPoint.y = Mathf.Min(bounds.max.y, bandTop);
            Vector3 toTarget = aimPoint - eye;

            // Only the horizontal angle is checked: the old 3D angle counted looking down as turning away,
            // so a crouching player right in front of the eye fell outside the cone.
            Vector3 flatToTarget = toTarget;
            flatToTarget.y = 0f;
            if (Vector3.Angle(forward, flatToTarget) >= viewAngle / 2f)
                continue;

            if (Physics.Raycast(eye, toTarget.normalized, toTarget.magnitude, obstacleMask))
                continue;

            if (!visibleTargets.Contains(col.transform))
                visibleTargets.Add(col.transform);
        }
    }
}