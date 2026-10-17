/// <summary>Get the morse code and return safely. Completes the moment the morse code is picked up.</summary>
public class Mission1 : Mission
{
    public Mission1()
        : base("Mission1", "Get the morse code and return safely.", MissionType.Main, SceneConstants.MISSION1_NAME)
    {
    }

    protected override void OnAccepted()
    {
        UIEvents.ShowTip("Mission Accepted");
        WorldEvents.ItemCollected += HandleItemCollected;
    }

    protected override void OnCompleted()
    {
        WorldEvents.ItemCollected -= HandleItemCollected;
        UIEvents.ShowTip($"{Name} completed");
    }

    private void HandleItemCollected(Item item)
    {
        if (item is MorseCode)
            Complete();
    }
}
