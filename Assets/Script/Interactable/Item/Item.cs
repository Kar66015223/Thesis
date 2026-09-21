using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    public GameObject Owner { get; set; }
    [field: SerializeField] public ItemData data { get; private set; }

    public bool canInteract = true;

    void Awake()
    {
        Owner = gameObject;
    }

    public bool CanInteract()
    {
        return canInteract;
    }

    public void Interact(GameObject interactor)
    {
        if (!canInteract)
            return;

        Debug.Log($"{data.itemName} was interacted by {interactor.name}");
    }
}