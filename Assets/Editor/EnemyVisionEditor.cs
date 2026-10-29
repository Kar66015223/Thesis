#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyVision))]
public class EnemyVisionEditor : Editor
{
    private static readonly Color BandColor = new(1f, 1f, 1f, 0.5f);

    void OnSceneGUI()
    {
        EnemyVision fov = (EnemyVision)target;

        Vector3 origin = fov.EyePosition;
        float radius = fov.ViewRadius;
        Vector3 viewAngleA = fov.DirFromAngle(-fov.ViewAngle / 2, false);
        Vector3 viewAngleB = fov.DirFromAngle(fov.ViewAngle / 2, false);

        Handles.color = Color.white;
        Handles.DrawWireArc(origin, Vector3.up, Vector3.forward, 360, radius);
        Handles.DrawLine(origin, origin + viewAngleA * radius);
        Handles.DrawLine(origin, origin + viewAngleB * radius);

        Vector3 halfHeight = Vector3.up * fov.ViewVerticalSize / 2f;
        Vector3 bottom = origin - halfHeight;
        Vector3 top = origin + halfHeight;

        Handles.color = BandColor;
        DrawSector(bottom, viewAngleA, viewAngleB, fov.ViewAngle, radius);
        DrawSector(top, viewAngleA, viewAngleB, fov.ViewAngle, radius);
        Handles.DrawLine(bottom, top);
        Handles.DrawLine(bottom + viewAngleA * radius, top + viewAngleA * radius);
        Handles.DrawLine(bottom + viewAngleB * radius, top + viewAngleB * radius);

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

    private static void DrawSector(Vector3 center, Vector3 edgeA, Vector3 edgeB, float angle, float radius)
    {
        Handles.DrawWireArc(center, Vector3.up, edgeA, angle, radius);
        Handles.DrawLine(center, center + edgeA * radius);
        Handles.DrawLine(center, center + edgeB * radius);
    }
}
#endif
