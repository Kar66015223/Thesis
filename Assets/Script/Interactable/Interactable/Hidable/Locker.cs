using UnityEngine;

public class Locker : Hidable
{
    private static readonly int ClosedState = Animator.StringToHash("Closed");
    private static readonly int LockerEnterHash = Animator.StringToHash("LockerEnter");

    [Tooltip("The copy of the player inside this locker that plays the get-in animation. Keep it inactive in the prefab.")]
    [SerializeField] private GameObject hidingCharacter;

    // The door's Animator sits on the same GameObject as this Locker.
    private Animator doorAnim;

    void Awake() => TryGetComponent(out doorAnim);

    protected override void OnEnterHiding()
    {
        // Start the character and the door on the same frame so they stay in sync.
        if (hidingCharacter != null)
        {
            // Turning it off and on again restarts its animation from the first frame.
            hidingCharacter.SetActive(false);
            hidingCharacter.SetActive(true);
        }

        if (doorAnim != null)
            doorAnim.SetTrigger(LockerEnterHash);
    }           

    protected override void OnExitHiding()
    {
        if (hidingCharacter != null)
            hidingCharacter.SetActive(false);

        // Put the door back to closed, in case the player left before the door finished closing.
        if (doorAnim != null)
            doorAnim.Play(ClosedState, 0, 0f);
    }
}
