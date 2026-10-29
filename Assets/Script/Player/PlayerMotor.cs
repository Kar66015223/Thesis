using UnityEngine;

public class PlayerMotor
{
    private const float GroundedStickVelocity = -2f;

    private readonly CharacterController body;
    private float verticalVelocity;

    public PlayerMotor(CharacterController body) => this.body = body;

    public void Move(Vector3 horizontalVelocity, float gravity, float deltaTime)
    {
        if (body.isGrounded && verticalVelocity < 0f)
            verticalVelocity = GroundedStickVelocity;
        else
            verticalVelocity += gravity * deltaTime;

        if (body.enabled)
            body.Move((horizontalVelocity + Vector3.up * verticalVelocity) * deltaTime);
    }

    public void Displace(Vector3 delta)
    {
        if (body.enabled)
            body.Move(delta);
    }

    public void SetStance(float centerY, float height)
    {
        body.center = new(body.center.x, centerY, body.center.z);
        body.height = height;
    }

    public void Teleport(Vector3 position)
    {
        bool wasEnabled = body.enabled;
        body.enabled = false;
        body.transform.position = position;
        body.enabled = wasEnabled;
    }

    public void SetEnabled(bool enabled) => body.enabled = enabled;
}