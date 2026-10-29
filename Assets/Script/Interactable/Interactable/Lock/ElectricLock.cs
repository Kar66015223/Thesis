using UnityEngine;

public class ElectricLock : Lock
{
    [SerializeField] private bool canUnlock = false;
    [SerializeField] private Transform unlockableSide;

    void OnEnable() => WorldEvents.LightsOutChanged += SetCanUnlock;
    void OnDisable() => WorldEvents.LightsOutChanged -= SetCanUnlock;

    public override bool TryUnlock(GameObject interactor)
    {
        if (IsOnSide(interactor, unlockableSide) || canUnlock)
        {
            SetLocked(false);
            return true;
        }
        
        UIEvents.ShowTip("The morse code must be here, I need to find a way to get inside.");
        return false;
    }

    private void SetCanUnlock(bool isLightOut) => canUnlock = isLightOut;
}