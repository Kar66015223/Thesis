using UnityEngine;

public abstract class Hidable : Interactable
{
    [SerializeField] private bool isHiding = false;
    [SerializeField] private Transform enterPos;
    [SerializeField] private Transform exitPos;

    public override void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor))
            return;

        base.Interact(interactor);
        ToggleHide(interactor);
    }
    
    public void ToggleHide(GameObject interactor)
    {
        if (interactor.TryGetComponent(out PlayerController controller))
        {
            isHiding = !isHiding;

            Vector3 targetPos = isHiding ? enterPos.position : exitPos.position;
            controller.SetHiding(isHiding, this, targetPos);
        }
    }
}
