using TMPro;
using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    public GameObject Owner { get; set; }
    [field: SerializeField] public Transform InteractPromptPos { get; private set; }
    [SerializeField] private string interactPromptText = "[Space]";

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

    public void TogglePrompt(TMP_Text prompt, bool isShow, GameObject player)
    {
        if (!CanInteract(player) && isShow)
            return;

        if (InteractPromptPos == null)
            return;

        Camera cam = Camera.main;

        prompt.enabled = isShow;

        if (prompt.enabled)
        {
            prompt.text = interactPromptText;
            prompt.transform.position = cam.WorldToScreenPoint(InteractPromptPos.position);
        }
        else
            prompt.text = null;
    }
}