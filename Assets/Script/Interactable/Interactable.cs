using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    public GameObject Owner { get; set; }
    [SerializeField] private GameObject interactPrompt;

    void Awake()
    {
        Owner = gameObject;
    }

    public virtual bool CanInteract(GameObject interactor)
    {
        if (interactor.TryGetComponent(out PlayerController controller))
        {
            if (controller.CurrentState == PlayerState.Running ||
                controller.CurrentState == PlayerState.UsingSkill ||
                controller.CurrentState == PlayerState.Hiding)
                return false;
        }

        return true;
    }

    public virtual void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor))
            return;

        Debug.Log($"{Owner.name} was interacted by {interactor.name}!!!!!!!!!!!!!!!!!!");
    }

    public void TogglePrompt(GameObject player, bool isShow)
    {
        if (!CanInteract(player) && isShow)
            return;

        if (interactPrompt == null)
            return;

        interactPrompt.SetActive(isShow);
    }
}