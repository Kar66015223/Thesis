using UnityEngine;

/// <summary>What a skill may do to its user. PlayerController implements it.</summary>
public interface ISkillUser
{
    Vector3 Position { get; }
    Vector3 Forward { get; }
    bool IsHiding { get; }
    void SetCanMove(bool canMove);
    /// <summary>Script-driven displacement that still respects collisions.</summary>
    void Dash(Vector3 delta);
    /// <summary>Turns the visible body to face a flat direction, even while movement is locked.</summary>
    void FaceDirection(Vector3 direction);
}

/// <summary>References resolved once and shared by every skill of one user.</summary>
public class SkillContext
{
    public SkillContext(GameObject user)
    {
        User = user;
        Body = user.GetComponent<ISkillUser>();
        Struggle = user.GetComponent<PlayerStruggle>();
    }

    public GameObject User { get; }
    public ISkillUser Body { get; }
    /// <summary>Lets a skill make the user uncatchable while it runs (e.g. mid-tackle). May be null.</summary>
    public PlayerStruggle Struggle { get; }
}
