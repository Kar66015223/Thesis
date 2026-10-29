using UnityEngine;

public class HideoutMainDoor : Interactable
{
    protected override void OnInteract(GameObject interactor)
    {
        MissionManager missions = MissionManager.Instance;
        if (missions == null)
            return;

        Mission mission = missions.ActiveMission;
        if (mission != null)
            SceneLoader.LoadScene(mission.SceneName);
        else
            UIEvents.ShowTip("Accept mission first");
    }
}
