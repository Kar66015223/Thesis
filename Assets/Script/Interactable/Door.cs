using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public GameObject Owner { get; set; }

    void Awake()
    {
        Owner = gameObject;
    }

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        Debug.Log("door interacted");
    }
}