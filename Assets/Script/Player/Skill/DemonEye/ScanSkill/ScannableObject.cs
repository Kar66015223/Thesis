using UnityEngine;

public class ScannableObject : MonoBehaviour, IScannable
{
    [Header("Highlight Settings")]
    [Tooltip("ชื่อ Layer ที่ตั้งไว้ใน URP Render Objects (ต้องพิมพ์ให้ตรงกัน) - must match the URP Render Objects layer")]
    [SerializeField] private string highlightLayerName = "RenderOnTop";

    private LayerHighlighter highlighter;

    private LayerHighlighter Highlighter => highlighter ??= new LayerHighlighter(gameObject, highlightLayerName);

    public void OnScanEnter() => Highlighter.Apply();
    public void OnScanExit() => Highlighter.Remove();
}
