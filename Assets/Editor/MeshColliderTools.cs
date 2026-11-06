#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class MeshColliderTools
{
    [MenuItem("Tools/For Your Tomorrow/Add Mesh Collider to selection")]
    private static void AddToSelection()
    {
        int added = 0;
        foreach (GameObject selected in Selection.gameObjects)
        {
            foreach (MeshFilter filter in selected.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh == null || filter.TryGetComponent(out Collider _))
                    continue;

                Undo.AddComponent<MeshCollider>(filter.gameObject).sharedMesh = filter.sharedMesh;
                added++;
            }
        }

        Debug.Log($"Added {added} Mesh Colliders");
    }

    [MenuItem("Tools/For Your Tomorrow/Add Mesh Collider to selection", true)]
    private static bool CanAdd() => Selection.gameObjects.Length > 0;
}
#endif