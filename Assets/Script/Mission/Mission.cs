using UnityEngine;

public abstract class Mission
{
    public string missionName;
    public string missionDesc;
    public MissionType missionType;
    public MissionStatus missionStatus;
    public GameObject uiButton;

    public abstract void OnAccept();
    public abstract void CheckCompletion();
    public abstract void Complete();
    public abstract void OnSubmit();
}