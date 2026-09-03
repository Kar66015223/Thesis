using UnityEngine;

public class Test : MonoBehaviour, IInteractable
{
    private GameObject _owner;
    public GameObject Owner { get => _owner; set => _owner = gameObject; }

    public bool canInteract = true;

    void Awake()
    {
        Owner = gameObject;
    }

    public bool CanInteract()
    {
        return canInteract;
    }

    public void Interact()
    {
        if (!CanInteract())
            return;

        Debug.Log($"{Owner.name} was Interacted!!!");
    }
}