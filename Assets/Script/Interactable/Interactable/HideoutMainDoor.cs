using UnityEngine;

public class HideoutMainDoor : Interactable
{
    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);

        if (MissionManager.instance == null)
            return;

        if (MissionManager.instance.GetOnProgressMission() != null)
            SceneLoader.LoadScene("Mission1");
        else
            GameEvent.OnShowTips?.Invoke("Accept mission first");
    }
}