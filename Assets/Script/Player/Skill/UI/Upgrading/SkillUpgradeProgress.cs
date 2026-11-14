using System.Collections.Generic;
using System;
using UnityEngine;

public static class SkillUpgradeProgress
{
    private static readonly Dictionary<SkillUpgradeData, int> progress = new();
    private static readonly List<SkillUpgradeData> tracked = new();
    private static readonly HashSet<SkillUpgradeData> learned = new();

    public static event Action<SkillUpgradeData> Changed;
    public static event Action<SkillUpgradeData> Completed;
    public static event Action<SkillUpgradeData> Unlocked;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        progress.Clear();
        tracked.Clear();
        learned.Clear();
    }

    public static void Track(SkillUpgradeData data)
    {
        if (data != null && !tracked.Contains(data))
            tracked.Add(data);
    }

    public static int Get(SkillUpgradeData data) =>
        data != null && progress.TryGetValue(data, out int value) ? value : 0;

    public static bool IsComplete(SkillUpgradeData data) => data != null && Get(data) >= data.maxProgress;
    public static bool IsUnlocked(SkillUpgradeData data) => data != null && learned.Contains(data);
    public static bool CanLearn(SkillUpgradeData data) => IsComplete(data) && !IsUnlocked(data);

    public static bool Learn(SkillUpgradeData data)
    {
        if (!CanLearn(data))
            return false;

        learned.Add(data);
        Unlocked?.Invoke(data);
        return true;
    }

    public static void Report(SkillUpgradeTrigger trigger, int amount = 1)
    {
        foreach (SkillUpgradeData data in tracked)
        {
            if (data.trigger != trigger || IsUnlocked(data))
                continue;

            bool wasComplete = IsComplete(data);

            progress[data] = Get(data) + amount;
            Changed?.Invoke(data);

            if (!wasComplete && IsComplete(data))
                Completed?.Invoke(data);
        }
    }
}
