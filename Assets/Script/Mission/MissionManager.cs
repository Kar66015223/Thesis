using System.Collections.Generic;
using UnityEngine;

/// <summary>Keeps the mission list and the active mission alive across scenes.</summary>
public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    private readonly List<Mission> availableMissions = new();

    public IReadOnlyList<Mission> AvailableMissions => availableMissions;
    public Mission ActiveMission { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        availableMissions.Add(new Mission1());
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
}
