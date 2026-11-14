using UnityEngine;

public class SkillUpgradeStation : Interactable
{
    protected override void OnInteract(GameObject interactor)
    {
        UIEvents.ToggleSkillUpgrade(true);
    }
}