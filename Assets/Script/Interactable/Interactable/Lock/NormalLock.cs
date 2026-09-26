using UnityEngine;

public class NormalLock : Lock
{
    [SerializeField] private bool canUnlock = false;
    [SerializeField] private Transform unlockableSide;
    [SerializeField] private bool needKey = false;

    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);

        if (interactor.TryGetComponent(out PlayerController ctrl))
            canUnlock = ctrl.hasKey;

        Transform playerSide = CurrentDoor.GetClosestPoint(interactor.transform.position);
        bool isOnUnlockableSide = playerSide == unlockableSide;

        if ((!isOnUnlockableSide && needKey && canUnlock) || isOnUnlockableSide)
        {
            IsLocked = false;
            CurrentDoor.Interact(interactor);
            return;
        }

        if (!isOnUnlockableSide && !needKey && !canUnlock)
        {   
            GameEvent.OnShowTips?.Invoke("Can't open from this side.");
            return;
        }
        
        if (!isOnUnlockableSide && needKey && !canUnlock)
        {
            GameEvent.OnShowTips?.Invoke("Door locked, need key.");
            return;
        }
    }
}