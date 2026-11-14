using UnityEngine;

public enum SkillUpgradeTrigger { None, EnemyScanned }

[CreateAssetMenu(fileName = "NewSkillUpgrade", menuName = "Scriptable Objects/SkillUpgrade")]
public class SkillUpgradeData : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string upgradeDesc;

    public SkillUpgradeTrigger trigger;
    [Min(1)] public int maxProgress = 1;
    
    public Sprite upgradeUsageImg;
}