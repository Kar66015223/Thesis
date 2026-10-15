using UnityEngine;

/// <summary>
/// Swaps a whole hierarchy to a render layer and back.
/// Shared by enemy scan highlight and objective highlight, which used to duplicate this code.
/// </summary>
public class LayerHighlighter
{
    private readonly GameObject root;
    private readonly int originalLayer;
    private readonly int highlightLayer;

    public bool IsHighlighted { get; private set; }

    public LayerHighlighter(GameObject root, string highlightLayerName)
    {
        this.root = root;
        originalLayer = root.layer;
        highlightLayer = LayerMask.NameToLayer(highlightLayerName);

        if (highlightLayer == -1)
            Debug.LogError($"Layer '{highlightLayerName}' not found. Add it in Tags and Layers.", root);
    }

    public void Apply()
    {
        if (IsHighlighted || highlightLayer == -1)
            return;

        SetLayerRecursively(root.transform, highlightLayer);
        IsHighlighted = true;
    }

    public void Remove()
    {
        if (!IsHighlighted)
            return;

        SetLayerRecursively(root.transform, originalLayer);
        IsHighlighted = false;
    }

    private static void SetLayerRecursively(Transform target, int layer)
    {
        target.gameObject.layer = layer;
        foreach (Transform child in target)
            SetLayerRecursively(child, layer);
    }
}
