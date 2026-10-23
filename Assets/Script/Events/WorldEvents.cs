using System;

public static class WorldEvents
{
    public static event Action<bool> LightsOutChanged;
    public static void SetLightsOut(bool isLightsOut) => LightsOutChanged?.Invoke(isLightsOut);

    public static event Action<Item> ItemCollected;
    public static void RaiseItemCollected(Item item) => ItemCollected?.Invoke(item);

    public static event Action<bool> ObjectiveShow;
    public static void RaiseObjectiveShow(bool isShow) => ObjectiveShow?.Invoke(isShow);
    public static event Action<ObjectiveHighlight> ObjectiveCompleted;
    public static void RaiseObjectiveCompleted(ObjectiveHighlight objective) => ObjectiveCompleted?.Invoke(objective);
}