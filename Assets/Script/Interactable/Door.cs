using UnityEngine;

public class Door : Interactable
{
    private readonly int IsOpenHash = Animator.StringToHash("IsOpen");
    private Animator anim;
    private bool isOpen = false;

    void Awake()
    {
        Owner = gameObject;
        anim = GetComponentInChildren<Animator>();
    }

    // public bool CanInteract()
    // {
    //     return true;
    // }

    public override void Interact(GameObject interactor)
    {
        isOpen = !isOpen;
        anim.SetBool(IsOpenHash, isOpen);
        Debug.Log($"door interacted by {interactor.name}");
    }
}