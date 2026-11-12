using UnityEngine;

public class ScanInteractTest : MonoBehaviour, IScanInteractable
{
    public GameObject Owner => gameObject;

    public bool CanInteract()
    {
        return true;
    }

    public void Interact()
    {
        Debug.Log("Interacted");
    }
}