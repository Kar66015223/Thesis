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
        Handles.DrawWireArc(origin, Vector3.up, Vector3.forward, 360, fov.viewRadius);

        Vector3 viewAngleA = fov.DirFromAngle(-fov.viewAngle / 2, false);
        Vector3 viewAngleB = fov.DirFromAngle(fov.viewAngle / 2, false);

        Handles.DrawLine(origin, origin + viewAngleA * fov.viewRadius);
        Handles.DrawLine(origin, origin + viewAngleB * fov.viewRadius);

        Handles.color = Color.red;
        foreach(Transform visibleTarget in fov.VisibleTargets)
        {
            Vector3 targetDrawPos = visibleTarget.position;
            if (visibleTarget.TryGetComponent(out Collider col))
            {
                targetDrawPos = col.bounds.center;
                targetDrawPos.y = col.bounds.max.y;
            }

            Handles.DrawLine(origin, targetDrawPos);
        }
    }
}