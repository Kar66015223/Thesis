public enum SkillType { Physical, DemonEye }
public enum SkillUsage { Press, Hold, Both }

public abstract class Skill
{
    public string skillName;
    public string skillDesc;
    public SkillType type;
    public SkillUsage usage;
    public float coolDown;
    public float cost;
    public SkillContext context;

    public abstract void Use();
    public abstract void HoldUse(bool isHolding);
    public abstract void HandleUse();
    public virtual void Tick(float deltaTime) {}
}