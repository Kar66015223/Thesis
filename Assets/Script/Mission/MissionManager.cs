using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    private readonly List<Mission> availableMissions = new();

    public IReadOnlyList<Mission> AvailableMissions => availableMissions;
    public Mission ActiveMission { get; private set; }

    [SerializeField] private ObjectiveData mission1ObjectiveData;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        availableMissions.Add(new Mission1(mission1ObjectiveData));
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Accept(Mission mission)
    {
        if (mission == null || mission.Status != MissionStatus.Pending)
            return;

        ActiveMission = mission;
        mission.Accept();
    }

    public void Restore(IReadOnlyDictionary<Mission, MissionStatus> statuses, Mission activeMission)
    {
        foreach (KeyValuePair<Mission, MissionStatus> saved in statuses)
            saved.Key.RestoreStatus(saved.Value);

        ActiveMission = activeMission;
    }
}
