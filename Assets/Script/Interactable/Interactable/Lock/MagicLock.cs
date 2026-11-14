using UnityEngine;

public class MagicLock : Lock, IScanInteractable
{
    public GameObject Owner => gameObject;
    [SerializeField] private float holdTime = 1.5f;
    public float HoldTime => holdTime;

    public bool CanInteract()
    {
        return isActiveAndEnabled;
    }

    public void Interact(GameObject interactor)
    {
        Unlock();
    }

    private void Unlock()
    {
        SetLocked(false);
        Owner.SetActive(false);
    }

    public override bool TryUnlock(GameObject interactor)
    {
        return false;
    }
}