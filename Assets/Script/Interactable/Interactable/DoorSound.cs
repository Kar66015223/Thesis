using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DoorSound : MonoBehaviour
{
    private AudioSource source;
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;
    [SerializeField] private AudioClip lockedClip;

    void Awake() => source = GetComponent<AudioSource>();

    public void PlayOpenSound() => source.PlayOneShot(openClip);
    public void PlayCloseSound() => source.PlayOneShot(closeClip);
    public void PlayLockedSound() => source.PlayOneShot(lockedClip);
}