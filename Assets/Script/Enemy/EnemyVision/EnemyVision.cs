using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class EnemyVision : MonoBehaviour
{
    public float viewRadius;
    [Range(0, 360)] public float viewAngle;
    public float viewHeight;

    public Vector3 EyePosition => transform.position + Vector3.up * viewHeight;

    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    [field: SerializeField] public List<Transform> VisibleTargets { get; private set; } = new();

    void Start()
    {
        StartCoroutine(FindTargetsWithDelay(0.5f));
    }

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
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if(Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if(!Physics.Raycast(transform.position, dirToTarget, distanceToTarget, obstacleMask))
                {
                    VisibleTargets.Add(target);
                }
            }
        }
    }

    public Vector3 DirFromAngle(float angleInDegree, bool angleIsGlobal)
    {
        if(!angleIsGlobal)
        {
            angleInDegree += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegree * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegree * Mathf.Deg2Rad));
    }
}