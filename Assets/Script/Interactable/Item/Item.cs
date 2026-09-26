using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    public GameObject Owner { get; set; }
    [field: SerializeField] public ItemData Data { get; private set; }

    void Awake()
    {
        Owner = gameObject;
    }

    public virtual bool CanInteract(GameObject interactor)
    {
        if (interactor != null)
        {
            if (interactor.TryGetComponent(out PlayerController controller))
            {
                if (controller.CurrentState == PlayerState.Running ||
                    controller.CurrentState == PlayerState.UsingSkill ||
                    controller.CurrentState == PlayerState.Hiding)
                    return false;
            }
        }

        return true;
    }

    public virtual void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor))
            return;

        Debug.Log($"{Data.itemName} was interacted by {interactor.name}");
    }
}