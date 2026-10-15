using UnityEngine;

/// <summary>Opens freely from <see cref="unlockableSide"/>; from the other side only while the power is out.</summary>
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

        UIEvents.ShowTip("Can't open, need to cut the power first");
        return false;
    }

    private void SetCanUnlock(bool isLightsOut) => canUnlock = isLightsOut;
}
