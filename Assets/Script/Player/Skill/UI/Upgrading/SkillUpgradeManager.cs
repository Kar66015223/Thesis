using System.Collections.Generic;
using UnityEngine;

public class SkillUpgradeManager : MonoBehaviour
{
    [SerializeField] private List<SkillUpgradeData> availableUpgrades = new();
    public IReadOnlyList<SkillUpgradeData> AvailableUpgrades => availableUpgrades;

    void Awake()
    {
        foreach (SkillUpgradeData data in availableUpgrades)
            SkillUpgradeProgress.Track(data);
    }
}