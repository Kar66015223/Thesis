using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    public GameObject Owner { get; set; }
    [field: SerializeField] public ItemData Data { get; private set; }
    [Min(1)] [SerializeField] private int amount = 1;

    private void Awake()
    {
        Owner = gameObject;
    }

    public virtual bool CanInteract(GameObject interactor)
    {
        if (interactor == null || Data == null) return false;
        Inventory inventory = interactor.GetComponent<Inventory>();
        if (inventory == null)
            inventory = interactor.GetComponentInChildren<Inventory>();
        return inventory != null;
    }

    public virtual void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;

        Inventory inventory = interactor.GetComponent<Inventory>();
        if (inventory == null)
            inventory = interactor.GetComponentInChildren<Inventory>();
        if (inventory == null) return;

        // เก็บไอเทมเข้ากระเป๋าได้ครบเท่านั้นจึงลบวัตถุในฉาก
        if (inventory.AddItem(Data, Mathf.Max(1, amount)))
            Destroy(gameObject);
    }
}
