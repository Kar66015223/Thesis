using System.Collections.Generic;

/// <summary>
/// Player and level state captured by <see cref="CheckpointManager.Save"/> when a checkpoint is reached.
/// Holds references to ItemData assets and Mission objects, so it only lives for the current play session.
/// </summary>
public class CheckpointData
{
    /// <summary>Index-aligned with <see cref="InventoryModel.Slots"/>, so the slot layout is kept too.</summary>
    public (ItemData item, int stack)[] InventorySlots { get; set; } = new (ItemData, int)[0];
    public Dictionary<Mission, MissionStatus> MissionStatuses { get; } = new();
    public Mission ActiveMission { get; set; }
    public float ScannerCooldownRemaining { get; set; }
    public int Money { get; set; }

    public Dictionary<EnemyController, int> EnemyRouteSteps { get; set; } = new();
    public bool IsLightsOut { get; set; }
    public int ObjectiveIndex { get; set; }
    public Dictionary<Door, (bool isOpen, bool isLocked)> DoorStates { get; } = new();
}
