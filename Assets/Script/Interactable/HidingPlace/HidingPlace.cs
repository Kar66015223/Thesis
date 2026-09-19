using UnityEngine;

public abstract class HidingPlace : MonoBehaviour
{
    [SerializeField] private bool isHiding = false;
    [SerializeField] private Transform enterPos;
    [SerializeField] private Transform exitPos;
    
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
