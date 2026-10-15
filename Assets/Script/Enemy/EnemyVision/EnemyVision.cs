using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [SerializeField] private float defaultViewRadius = 10f;
    [SerializeField] private float viewRadiusChasing = 30f;
    [SerializeField] private float scanInterval = 0.1f;

    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    private readonly Collider[] overlapBuffer = new Collider[MaxTargetsPerScan];
    private readonly List<Transform> visibleTargets = new();
    private readonly List<Transform> confirmedTargets = new();

    private EnemyController controller;
    private Coroutine scanRoutine;
    private float confirmTimer;

    public float ViewRadius => viewRadius;
    public float ViewAngle => viewAngle;
    public Vector3 EyePosition => transform.position + Vector3.up * viewHeight;
    public IReadOnlyList<Transform> VisibleTargets => visibleTargets;
    public IReadOnlyList<Transform> ConfirmedTargets => confirmedTargets;

    void Awake() => controller = GetComponent<EnemyController>();

    void OnEnable() => EnableVision();

    // Unity stops coroutines on disable; forget the handle so OnEnable can restart scanning.
    void OnDisable() => scanRoutine = null;

    void Update()
    {
        if (visibleTargets.Count > 0)
            UpdateConfirmation(visibleTargets[0]);
        else if (confirmTimer > 0f)
            ResetConfirmation();

        viewRadius = controller.IsChasing ? viewRadiusChasing : defaultViewRadius;
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

        return new Vector3(Mathf.Sin(angleInDegree * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegree * Mathf.Deg2Rad));
    }

    private void UpdateConfirmation(Transform target)
    {
        // First glimpse...
        if (confirmTimer == 0f)
            controller.OnSeenTarget(target);

        confirmTimer += Time.deltaTime;
        float progress = controller.ConfirmWaitTime > 0f ? Mathf.Clamp01(confirmTimer / controller.ConfirmWaitTime) : 1f;
        EnemyEvents.RaiseConfirmProgressChanged(transform, progress);

        // ...then confirmed after looking long enough.
        if (confirmTimer >= controller.ConfirmWaitTime && !confirmedTargets.Contains(target))
        {
            confirmedTargets.Add(target);
            controller.OnSeenTarget(target);
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
        int count = Physics.OverlapSphereNonAlloc(eye, viewRadius, overlapBuffer, targetMask);

        for (int i = 0; i < count; i++)
        {
            Collider col = overlapBuffer[i];

            Vector3 targetTop = col.bounds.center;
            targetTop.y = col.bounds.max.y;

            Vector3 toTarget = targetTop - eye;
            if (Vector3.Angle(transform.forward, toTarget) >= viewAngle / 2f)
                continue;

            if (Physics.Raycast(eye, toTarget.normalized, toTarget.magnitude, obstacleMask))
                continue;

            if (!visibleTargets.Contains(col.transform))
                visibleTargets.Add(col.transform);
        }
    }
}
