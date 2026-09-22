using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    public GameObject Owner { get; set; }
    [field: SerializeField] public ItemData data { get; private set; }

    void Awake()
    {
        Owner = gameObject;
    }

    public bool CanInteract(GameObject interactor)
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

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor))
            return;

        Debug.Log($"{data.itemName} was interacted by {interactor.name}");
    }
}