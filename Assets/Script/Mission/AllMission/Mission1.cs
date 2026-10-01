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
        if (missionStatus != MissionStatus.Accepted)
        {
            MissionManager.instance.SetOnProgressMission(this);
            GameEvent.OnShowTips?.Invoke("Mission Accepted");
            missionStatus = MissionStatus.OnProgress;
        }
    }

    public override void CheckCompletion()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if(player.TryGetComponent(out PlayerController controller))
            if(controller.hasMorseCode)
                Complete();
    }

    public override void Complete()
    {
        missionStatus = MissionStatus.Completed;
        GameEvent.OnShowTips?.Invoke($"{missionName} completed");
    }

    public override void OnSubmit()
    {
        throw new System.NotImplementedException();
    }
}