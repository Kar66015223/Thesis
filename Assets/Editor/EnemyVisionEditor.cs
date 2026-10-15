#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyVision))]
public class EnemyVisionEditor : Editor
{
    void OnSceneGUI()
    {
        EnemyVision fov = (EnemyVision)target;

        Vector3 origin = fov.EyePosition;
        Handles.color = Color.white;
        Handles.DrawWireArc(origin, Vector3.up, Vector3.forward, 360, fov.ViewRadius);

        Vector3 viewAngleA = fov.DirFromAngle(-fov.ViewAngle / 2, false);
        Vector3 viewAngleB = fov.DirFromAngle(fov.ViewAngle / 2, false);

        Handles.DrawLine(origin, origin + viewAngleA * fov.ViewRadius);
        Handles.DrawLine(origin, origin + viewAngleB * fov.ViewRadius);

        Handles.color = Color.red;
        foreach (Transform confirmed in fov.ConfirmedTargets)
        {
            if (confirmed == null)
                continue;

            Vector3 targetDrawPos = confirmed.position;
            if (confirmed.TryGetComponent(out Collider col))
            {
                targetDrawPos = col.bounds.center;
                targetDrawPos.y = col.bounds.max.y;
            }

            Handles.DrawLine(origin, targetDrawPos);
        }
    }
}
#endif
