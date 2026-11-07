using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class GeneralTools
{
    private const string parentName = "Interactable";

    [MenuItem("Tools/For Your Tomorrow/Group all interactables")]
    private static void GroupAllInteractables()
    {
        int added = 0;
        Transform found = null;

        Transform[] allTransform = StageUtility.GetCurrentStageHandle().FindComponentsOfType<Transform>();
        foreach (var transform in allTransform)
        {
            if (transform.gameObject.name == parentName)
            {
                found = transform;
                break;
            }
        }
        
        if (found == null)
        {
            Debug.LogWarning($"Can't find object named {parentName}");
            return; 
        }
        Transform parent = found.transform;

        foreach (Transform selected in Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable))
        {
            if (!IsValid(selected, parent))
                continue;

            Undo.SetTransformParent(selected, parent, "Group Interactables");
            added++;
        }

        Debug.Log($"Added {added} objects to {parent.gameObject.name}");
    }

    private static bool IsValid(Transform obj, Transform parent) =>
        obj.parent != parent
        && obj != parent
        && !parent.IsChildOf(obj)
        && obj.gameObject.scene == parent.gameObject.scene
        && obj.GetComponentInChildren<Interactable>(true) != null;
}