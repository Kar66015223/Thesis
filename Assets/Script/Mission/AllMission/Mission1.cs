using UnityEngine;

public class Mission1 : Mission
{
    public Mission1()
    {
        missionName = "Mission1";
        missionDesc = "Get the morse code and return safely.";
        missionType = MissionType.Main;
        missionStatus = MissionStatus.Pending;
    }

    public override void OnAccept()
    {
        MissionManager.instance.SetOnProgressMission(this);
        Debug.Log("Mission Accepted!!!");
    }

    public override void CheckCompletion()
    {
        throw new System.NotImplementedException();
    }

    public override void Complete()
    {
        throw new System.NotImplementedException();
    }

    public override void OnSubmit()
    {
        throw new System.NotImplementedException();
    }
}