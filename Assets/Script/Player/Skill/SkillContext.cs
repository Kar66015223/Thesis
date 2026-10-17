using UnityEngine;

/// <summary>What a skill may do to its user. PlayerController implements it.</summary>
public interface ISkillUser
{
    Vector3 Position { get; }
    Vector3 Forward { get; }
    void SetCanMove(bool canMove);
    /// <summary>Script-driven displacement that still respects collisions.</summary>
    void Dash(Vector3 delta);
}

/// <summary>References resolved once and shared by every skill of one user.</summary>
public class SkillContext
{
    public SkillContext(GameObject user)
    {
        User = user;
        Body = user.GetComponent<ISkillUser>();
    }

    public GameObject User { get; }
    public ISkillUser Body { get; }
}
