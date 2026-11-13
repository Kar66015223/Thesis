using UnityEngine;

public class ScanTakeableItem : MonoBehaviour, IScanInteractable
{
    public GameObject Owner => gameObject;

    [SerializeField] private float holdTime = 1.5f;
    public float HoldTime => holdTime;

    private Item item;

    void Awake() => item = GetComponent<Item>();
    public bool CanInteract() => isActiveAndEnabled && item.isActiveAndEnabled;
    public void Interact(GameObject interactor) => item.Interact(interactor);
}