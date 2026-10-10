using UnityEngine;

public class ObjectiveHighlight : MonoBehaviour
{
    public const string layerName = "ObjectiveHighlight";
    private int highlightLayerIndex;
    private int originalLayer;

    private bool isHighlighted = false;

    void Start()
    {
        originalLayer = gameObject.layer;

        highlightLayerIndex = LayerMask.NameToLayer(layerName);
        if (highlightLayerIndex == -1)
            Debug.LogError($"No layer: {layerName}!");
    }

    public void ApplyHighlight()
    {
        if (isHighlighted)
            return;

        SetLayerRecursively(gameObject, highlightLayerIndex);
        isHighlighted = true;
    }

    public void RemoveHighlight()
    {
        SetLayerRecursively(gameObject, originalLayer);
        isHighlighted = false;
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
