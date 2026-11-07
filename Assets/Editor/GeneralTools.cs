using UnityEngine;
using UnityEditor;

public static class GeneralTools
{
    private const string parentName = "Interactable";

    [MenuItem("Tools/For Your Tomorrow/Group all interactables")]
    private static void GroupAllInteractables()
    {
        int added = 0;
        
        Transform parent = GameObject.Find(parentName).transform;
        if (parent == null)
        {
            Debug.LogWarning($"Can't find object named {parentName}");
            return;
        }

        foreach (GameObject selected in Selection.gameObjects)
        {
            GameObject targetObj = selected.GetComponentInChildren<InteractableBase>(true).Owner;
            if (targetObj == null || targetObj.transform.parent == parent)
                continue;

            targetObj.transform.SetParent(parent);
            added++;
        }

        Debug.Log($"Added {added} objects to {parent.name}");
    }
}