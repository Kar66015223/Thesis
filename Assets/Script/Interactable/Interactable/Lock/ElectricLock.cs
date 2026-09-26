using UnityEngine;

public class ElectricLock : Lock
{
    [SerializeField] private bool canUnlock = false;
    [SerializeField] private Transform unlockableSide;

    void OnEnable()
    {
        GameEvent.OnLightsOut += SetCanUnlock;
    }

    void OnDisable()
    {
        GameEvent.OnLightsOut -= SetCanUnlock;
    }

    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);

        Transform playerSide = CurrentDoor.GetClosestPoint(interactor.transform.position);
        bool isOnUnlockableSide = playerSide == unlockableSide;

        if ((!isOnUnlockableSide && canUnlock) || isOnUnlockableSide)
        {
            IsLocked = false;
            CurrentDoor.Interact(interactor);
            return;
        }

        if (!isOnUnlockableSide && !canUnlock)
        {
            GameEvent.OnShowTips?.Invoke("Can't open, need to cut the power first");
            return;
        }
    }

    private void SetCanUnlock(bool flag) => canUnlock = flag;
}