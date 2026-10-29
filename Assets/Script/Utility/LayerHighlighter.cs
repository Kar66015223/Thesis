using System.Collections.Generic;
using UnityEngine;

public class LayerHighlighter
{
    private readonly GameObject root;
    private readonly int highlightLayer;

    private readonly List<(GameObject obj, int layer)> originalLayers = new();

    public bool IsHighlighted { get; private set; }

    public LayerHighlighter(GameObject root, string highlightLayerName)
    {
        this.root = root;
        highlightLayer = LayerMask.NameToLayer(highlightLayerName);

        if (highlightLayer == -1)
            Debug.LogError($"Layer '{highlightLayerName}' not found. Add it in Tags and Layers.", root);
    }

    public void Apply()
    {
        if (IsHighlighted || highlightLayer == -1)
            return;

        originalLayers.Clear();
        SetHighlightRecursively(root.transform);
        IsHighlighted = true;
    }

    public void Remove()
    {
        if (!IsHighlighted)
            return;

        foreach ((GameObject obj, int layer) in originalLayers)
        {
            if (obj != null)
                obj.layer = layer;
        }

        originalLayers.Clear();
        IsHighlighted = false;
    }

    private void SetHighlightRecursively(Transform target)
    {
        if (target is RectTransform)
            return;

        originalLayers.Add((target.gameObject, target.gameObject.layer));
        target.gameObject.layer = highlightLayer;

        foreach (Transform child in target)
            SetHighlightRecursively(child);
    }
}
