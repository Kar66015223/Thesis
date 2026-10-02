using System.Collections.Generic;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyController))]
public class EnemyVision : MonoBehaviour
{
    public float viewRadius;
    [Range(0, 360)] public float viewAngle;
    public float viewHeight;

    [SerializeField] private float defaultViewRadius = 10f;
    [SerializeField] private float viewRadiusChasing = 30f;

    public Vector3 EyePosition => transform.position + Vector3.up * viewHeight;

    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    [SerializeField] private float confirmTimer;

    [field: SerializeField] public List<Transform> VisibleTargets { get; private set; } = new();
    [field: SerializeField] public List<Transform> ConfirmedTargets { get; private set; } = new();

    private EnemyController controller;

    void Awake()
    {
        controller = GetComponent<EnemyController>();
    }

    void Start()
    {
        EnableVision();
    }

    void Update()
    {
        if (VisibleTargets.Count > 0)
        {
            Transform target = VisibleTargets[0].transform;

            if (confirmTimer == 0f)
                controller.OnSeenTarget(target);

            confirmTimer += Time.deltaTime;
            float fillRatio = Mathf.Clamp01(confirmTimer / controller.confirmWaitTime);

            GameEvent.OnUpdateConfirmTimer?.Invoke(transform, fillRatio);

            if (confirmTimer >= controller.confirmWaitTime && !ConfirmedTargets.Contains(target))
            {
                ConfirmedTargets.Add(target);
                controller.OnSeenTarget(target);
            }
        }
        else
        {
            if (confirmTimer > 0f)
            {
                confirmTimer = 0f;
                ConfirmedTargets.Clear();
                GameEvent.OnUpdateConfirmTimer?.Invoke(transform, 0f);
            }
        }

        UpdateViewRadiusChange();
    }

    public void EnableVision()
    {
        // DisableVision();
        StartCoroutine(FindTargetsWithDelay(0.1f));
    }
    
    public void DisableVision() => StopAllCoroutines();
    
    IEnumerator FindTargetsWithDelay(float delay)
    {
        while(true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    void FindVisibleTargets()
    {
        VisibleTargets.Clear();
        Collider[] targetsInView = Physics.OverlapSphere(EyePosition, viewRadius, targetMask);

        foreach(Collider col in targetsInView)
        {
            Transform target = col.transform;

            Vector3 targetCenter = col.bounds.center;
            targetCenter.y = col.bounds.max.y;

            Vector3 dirToTarget = (targetCenter - EyePosition).normalized;

            if(Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if(!Physics.Raycast(EyePosition, dirToTarget, distanceToTarget, obstacleMask))
                {
                    if(!VisibleTargets.Contains(target))
                        VisibleTargets.Add(target);
                }
            }
        }
    }

    public Vector3 DirFromAngle(float angleInDegree, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
            angleInDegree += transform.eulerAngles.y;

        return new Vector3(Mathf.Sin(angleInDegree * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegree * Mathf.Deg2Rad));
    }

    private void UpdateViewRadiusChange()
    {
        if (controller.CurrentState is ChaseState)
        {
            viewRadius = viewRadiusChasing;
        }
        else
            viewRadius = defaultViewRadius;
    }
}