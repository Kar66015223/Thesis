using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [Tooltip("Found automatically when left empty.")]
    [SerializeField] private PlayerController player;
    [Tooltip("Found automatically when left empty.")]
    [SerializeField] private EnemyManager enemyManager;
    [Tooltip("Found automatically when left empty.")]
    [SerializeField] private ObjectiveManager objectiveManager;

    private PlayerHealth health;
    private SkillManager skills;
    private readonly HashSet<Item> itemsSinceSave = new();
    private bool isLightsOut;
    private readonly List<Door> doors = new();

    public static CheckpointManager Instance { get; private set; }
    public Transform CurCheckpoint { get; private set; }
    public CheckpointData SavedState { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (player == null)
            player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            health = player.GetComponent<PlayerHealth>();
            skills = player.GetComponent<SkillManager>();
        }

        if (enemyManager == null)
            enemyManager = FindFirstObjectByType<EnemyManager>();
        if (objectiveManager == null)
            objectiveManager = FindFirstObjectByType<ObjectiveManager>();
    }

    void OnEnable()
    {
        PlayerEvents.OnCheckpoint += SetCheckpoint;
        WorldEvents.ItemCollected += TrackPickedUpItem;
        WorldEvents.LightsOutChanged += TrackLightsOut;
    }

    void OnDisable()
    {
        PlayerEvents.OnCheckpoint -= SetCheckpoint;
        WorldEvents.ItemCollected -= TrackPickedUpItem;
        WorldEvents.LightsOutChanged -= TrackLightsOut;
    }

    public void RegisterDoor(Door door)
    {
        if (door != null && !doors.Contains(door))
            doors.Add(door);
    }

    void TrackPickedUpItem(Item item) => itemsSinceSave.Add(item);
    void TrackLightsOut(bool lightsOut) => isLightsOut = lightsOut;

    void SetCheckpoint(Transform point)
    {
        if (CurCheckpoint == point)
            return;

        CurCheckpoint = point;
        Save();
    }

    public void Save()
    {
        if (player == null)
        {
            Debug.LogWarning("CheckpointManager: no PlayerController found, nothing to save.", this);
            return;
        }

        CheckpointData data = new()
        {
            InventorySlots = player.Inventory.Snapshot(),
            IsLightsOut = isLightsOut,
        };

        MissionManager missions = MissionManager.Instance;
        if (missions != null)
        {
            foreach (Mission mission in missions.AvailableMissions)
                data.MissionStatuses[mission] = mission.Status;

            data.ActiveMission = missions.ActiveMission;
        }

        if (skills != null)
            data.ScannerCooldownRemaining = skills.ScannerCooldownRemaining;
        if (EconomyManager.Instance != null)
            data.Money = EconomyManager.Instance.CurrentMoney;
        if (enemyManager != null)
            data.EnemyRouteSteps = enemyManager.SnapshotRouteSteps();
        if (objectiveManager != null)
            data.ObjectiveIndex = objectiveManager.CurrentIndex;

        foreach (Door door in doors)
        {
            if (door != null)
                data.DoorStates[door] = (door.IsOpen, door.IsLocked);
        }

        SavedState = data;
        itemsSinceSave.Clear();
    }

    public bool Load()
    {
        if (SavedState == null || CurCheckpoint == null || player == null)
            return false;

        player.Teleport(CurCheckpoint.position, CurCheckpoint.rotation);
        player.Inventory.Restore(SavedState.InventorySlots);

        foreach (Item item in itemsSinceSave)
        {
            if (item != null)
                item.gameObject.SetActive(true);
        }
        itemsSinceSave.Clear();

        MissionManager missions = MissionManager.Instance;
        if (missions != null)
            missions.Restore(SavedState.MissionStatuses, SavedState.ActiveMission);

        if (skills != null)
            skills.RestoreScannerCooldown(SavedState.ScannerCooldownRemaining);

        if (EconomyManager.Instance != null)
            EconomyManager.Instance.RestoreMoney(SavedState.Money);

        WorldEvents.SetLightsOut(SavedState.IsLightsOut);

        foreach (KeyValuePair<Door, (bool isOpen, bool isLocked)> saved in SavedState.DoorStates)
        {
            if (saved.Key != null)
                saved.Key.RestoreState(saved.Value.isOpen, saved.Value.isLocked);
        }

        if (objectiveManager != null)
            objectiveManager.RestoreObjective(SavedState.ObjectiveIndex);

        if (enemyManager != null)
            enemyManager.RestoreRouteSteps(SavedState.EnemyRouteSteps);

        if (health != null)
            health.Revive();

        PlayerEvents.RaiseRespawned();
        return true;
    }
}
