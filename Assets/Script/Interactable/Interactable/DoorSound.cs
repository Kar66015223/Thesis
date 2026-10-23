using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DoorSound : MonoBehaviour
{
    private AudioSource source;
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;
    [SerializeField] private AudioClip lockedClip;

    void Awake() => source = GetComponent<AudioSource>();

    public void PlayOpenSound() => AudioSource.PlayClipAtPoint(openClip, transform.position);
    public void PlayCloseSound() => AudioSource.PlayClipAtPoint(closeClip, transform.position);
    public void PlayLockedSound() => AudioSource.PlayClipAtPoint(lockedClip, transform.position);
}