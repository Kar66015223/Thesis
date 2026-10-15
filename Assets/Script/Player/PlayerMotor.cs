using UnityEngine;

/// <summary>
/// Moves the CharacterController: gravity, horizontal movement, stance (collider size) and teleporting.
/// Knows nothing about input, stamina or animation.
/// </summary>
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

    /// <summary>Moves without gravity, e.g. a dash.</summary>
    public void Displace(Vector3 delta)
    {
        if (body.enabled)
            body.Move(delta);
    }

    public void SetStance(float centerY, float height)
    {
        body.center = new Vector3(body.center.x, centerY, body.center.z);
        body.height = height;
    }

    /// <summary>A CharacterController overrides transform.position while enabled, so disable it around the move.</summary>
    public void Teleport(Vector3 position)
    {
        bool wasEnabled = body.enabled;
        body.enabled = false;
        body.transform.position = position;
        body.enabled = wasEnabled;
    }

    public void SetEnabled(bool enabled) => body.enabled = enabled;
}
