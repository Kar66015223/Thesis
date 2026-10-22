using UnityEngine;

// ใส่ , IInteractable เพื่อทำตามสัญญาของเพื่อน
public class InteractShopNPC : MonoBehaviour, IInteractable
{
    // 1. ระบุตัวเจ้าของ
    public GameObject Owner => gameObject;

    // 2. ระบุปุ่มที่จะใช้กด (สมมติว่าเป็นปุ่ม InteractButton.Action หรือตามที่เพื่อนตั้งไว้)
    [field: SerializeField] public InteractButton InteractButton { get; private set; }

    // 3. เงื่อนไข: คุยได้ไหม?
    public bool CanInteract(GameObject interactor)
    {
        // คุยได้เสมอ (ถ้าจะเพิ่มเงื่อนไข เช่น ต้องมีเลเวล 5 ก่อนถึงคุยได้ ก็มาเขียนเพิ่มตรงนี้)
        return true;
    }

    // 4. สิ่งที่จะเกิดขึ้นเมื่อกดคุย
    public void Interact(GameObject interactor)
    {
        // สั่งเปิดหน้าต่างร้านค้าที่เราทำไว้
        UIEvents.ToggleShopBoard(true);
    }
}