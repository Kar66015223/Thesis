using System.Collections.Generic;

public class CheckpointData
{
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
