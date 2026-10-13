using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    public Canvas inventoryCanvas; // ลาก Canvas มาวางใน Inspector

    void Start()
    {
        if (inventoryCanvas == null)
        {
            Debug.LogError("Inventory Canvas ไม่ถูกกำหนดใน Inspector!");
            return;
        }

        inventoryCanvas.enabled = false; // ปิด Canvas ตอนเริ่มเกม
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && inventoryCanvas != null)
        {
            inventoryCanvas.enabled = !inventoryCanvas.enabled; // เปิด/ปิด Canvas
        }
    }
}
