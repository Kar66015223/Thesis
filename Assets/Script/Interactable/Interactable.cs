using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    public GameObject Owner { get; set; }
    [SerializeField] private GameObject interactPrompt;

    void Awake()
    {
        Owner = gameObject;
    }

    public virtual bool CanInteract()
    {
        return true;
    }

    public virtual void Interact(GameObject interactor)
    {
        if (!CanInteract())
            return;

        Debug.Log($"{Owner.name} was interacted by {interactor.name}!!!!!!!!!!!!!!!!!!");
    }

    public void TogglePrompt(bool isShow)
    {
        if (!CanInteract())
            return;

        if (interactPrompt == null)
            return;

        interactPrompt.SetActive(isShow);
    }
}