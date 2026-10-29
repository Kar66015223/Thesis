using UnityEngine;

public enum SkillType { Physical, DemonEye }
public enum SkillUsage { Press, Hold, Both }

public abstract class Skill
{
    protected Skill(SkillData data, SkillContext context)
    {
        Data = data;
        Context = context;
    }

    public SkillData Data { get; }
    protected SkillContext Context { get; }

    public string Name => Data.skillName;
    public string Description => Data.skillDesc;
    public Sprite Icon => Data.icon;
    public SkillType Type => Data.type;
    public SkillUsage Usage => Data.usage;

    public virtual void OnInputPressed() { }
    public virtual void OnInputReleased() { }
    public virtual void Tick(float deltaTime) { }
}
