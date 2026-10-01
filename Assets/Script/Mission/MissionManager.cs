using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager instance;

    private List<Mission> availableMission = new();
    private Mission onProgressMission;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        availableMission.Add(new Mission1());
    }

    public List<Mission> GetAvailableMissions() => availableMission;
    public void SetOnProgressMission(Mission mission) => onProgressMission = mission;
}