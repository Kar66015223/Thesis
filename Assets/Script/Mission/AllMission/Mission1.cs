public class Mission1 : Mission
{
    public Mission1(ObjectiveData data)
        : base("Mission1", "Get the morse code and return safely.", MissionType.Main, SceneConstants.MISSION1_NAME, data)
    {
    }

    protected override void OnAccepted()
    {
        WorldEvents.ItemCollected += HandleItemCollected;
        UIEvents.RequestChangeObjective(Data);
    }

    protected override void OnCompleted()
    {
        WorldEvents.ItemCollected -= HandleItemCollected;
        UIEvents.ShowTip($"{Name} completed");
    }

    protected override void OnStatusRestored()
    {
        WorldEvents.ItemCollected -= HandleItemCollected;
        if (Status == MissionStatus.OnProgress)
            WorldEvents.ItemCollected += HandleItemCollected;
    }

    private void HandleItemCollected(Item item)
    {
        if (item is MorseCode)
            Complete();
    }
}
