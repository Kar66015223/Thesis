using UnityEngine;

public class PlayerAnimationEventHandler : MonoBehaviour
{
    private PlayerSound sound;

    void Awake() => sound = GetComponentInParent<PlayerSound>();

    public void PlayWalkSound() => sound.PlayWalkSound();
    public void PlayRunSound() => sound.PlayRunSound();
}
