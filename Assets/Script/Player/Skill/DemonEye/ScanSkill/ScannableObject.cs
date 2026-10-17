using UnityEngine;

/// <summary>Renders the object on top of walls while it is inside the Demon Eye scan.</summary>
public class ScannableObject : MonoBehaviour, IScannable
{
    [Header("Highlight Settings")]
    [Tooltip("ชื่อ Layer ที่ตั้งไว้ใน URP Render Objects (ต้องพิมพ์ให้ตรงกัน) - must match the URP Render Objects layer")]
    [SerializeField] private string highlightLayerName = "RenderOnTop";

    private LayerHighlighter highlighter;

    // Lazy so it works whatever the init order.
    private LayerHighlighter Highlighter => highlighter ??= new LayerHighlighter(gameObject, highlightLayerName);

    public void OnScanEnter() => Highlighter.Apply();
    public void OnScanExit() => Highlighter.Remove();
}
