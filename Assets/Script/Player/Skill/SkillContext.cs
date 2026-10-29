using UnityEngine;

public interface ISkillUser
{
    Vector3 Position { get; }
    Vector3 Forward { get; }
    bool IsHiding { get; }
    void SetCanMove(bool canMove);
    void Dash(Vector3 delta);
    void FaceDirection(Vector3 direction);
}

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
    public PlayerStruggle Struggle { get; }
}
