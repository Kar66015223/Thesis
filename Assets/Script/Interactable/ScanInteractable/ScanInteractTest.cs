using UnityEngine;

public class ScanInteractTest : MonoBehaviour, IScanInteractable
{
    public GameObject Owner => gameObject;
    [SerializeField] private float holdTime = 1.5f;
    public float HoldTime => holdTime;

    public bool CanInteract() => isActiveAndEnabled;

    public void Interact(GameObject interactor)
    {
        Debug.Log("Interacted");
    }
}