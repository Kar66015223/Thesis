using UnityEngine;

public class Test : MonoBehaviour, IInteractable
{
    private GameObject _owner;
    public GameObject Owner { get => _owner; set => _owner = gameObject; }

    public bool canInteract = true;

    public bool CanInteract()
    {
        if (canInteract)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Interact()
    {
        if (!CanInteract())
            return;

        Debug.Log("Interact!!!");
    }
}