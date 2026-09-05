using UnityEngine;
using System.Collections;

public class ScannableEnemy : MonoBehaviour
{
    [Header("Highlight Settings")]
    [Tooltip("ชื่อ Layer ที่ตั้งไว้ใน URP Render Objects (ต้องพิมพ์ให้ตรงกัน)")]
    public string highlightLayerName = "RenderOnTop"; 
    
    [Tooltip("ระยะเวลาที่ศัตรูจะเรืองแสงหลังโดนคลื่นสแกนชน (วินาที)")]
    public float highlightDuration = 3f;

    private int originalLayer;
    private int highlightLayerIndex;
    private bool isHighlighted = false;

    void Start()
    {
        // เก็บ Layer เดิมของศัตรูไว้
        originalLayer = gameObject.layer;
        
        // หาค่า Index ของ Layer ปลายทาง
        highlightLayerIndex = LayerMask.NameToLayer(highlightLayerName);
        if (highlightLayerIndex == -1)
        {
            Debug.LogError("หา Layer ที่ชื่อ " + highlightLayerName + " ไม่พบ! โปรดไปสร้างใน Tags and Layers");
        }
    }

    // ฟังก์ชันนี้จะทำงานเมื่อคลื่นสแกนที่ขยายตัวมาชนโดนตัวศัตรู
    void OnTriggerEnter(Collider other)
    {
        // เช็คว่าสิ่งที่มาชนมี Tag เป็น "ScannerWave" หรือไม่
        if (other.CompareTag("ScannerWave") && !isHighlighted)
        {
            StartCoroutine(ApplyHighlightEffect());
        }
    }

    private IEnumerator ApplyHighlightEffect()
    {
        isHighlighted = true;
        
        // 1. เปลี่ยน Layer เพื่อให้ Highlight Opaque (Render Objects) ทำงาน
        SetLayerRecursively(gameObject, highlightLayerIndex);

        // 2. รอเวลา (Duration ของสกิลที่มองเห็นศัตรู)
        yield return new WaitForSeconds(highlightDuration);

        // 3. หมดเวลา เปลี่ยน Layer กลับเป็นปกติ
        SetLayerRecursively(gameObject, originalLayer);
        
        isHighlighted = false;
    }

    // ฟังก์ชันย่อยสำหรับเปลี่ยน Layer ให้กับโมเดลลูกๆ ทั้งหมด (กรณีโมเดลศัตรูมีหลายชิ้นส่วน)
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}