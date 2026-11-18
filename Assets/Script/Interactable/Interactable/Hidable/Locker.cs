using UnityEngine;

public class Locker : Hidable
{
    private static readonly int LockerEnterHash = Animator.StringToHash("LockerEnter");
    private static readonly int LockerExitHash = Animator.StringToHash("LockerExit");

    [Tooltip("The copy of the player inside this locker that plays the get-in and get-out animations. Keep it inactive in the prefab.")]
    [SerializeField] private GameObject hidingCharacter;

    private Animator doorAnim;
    private Animator characterAnim;

    private Vector3 characterStartPos;
    private Quaternion characterStartRot;

    private bool isExiting;
    private int getInStateHash;

    public bool IsHide { get; private set; } = false;

    protected override bool CanExit =>
        IsHide && !isExiting && (characterAnim == null || IsCurrentStateFinished());

    void Awake()
    {
        TryGetComponent(out doorAnim);
        if (hidingCharacter != null)
        {
            hidingCharacter.TryGetComponent(out characterAnim);
            hidingCharacter.transform.GetLocalPositionAndRotation(out characterStartPos, out characterStartRot);
        }
    }

    void Update()
    {
        if (!isExiting)
            return;

        // Wait until the copy has moved on from the get-in state and finished the get-out clip.
        bool getOutFinished = characterAnim.GetCurrentAnimatorStateInfo(0).fullPathHash != getInStateHash
                              && IsCurrentStateFinished();
        if (!getOutFinished)
            return;

        isExiting = false;

        // Bring the real player out exactly where the copy ended up, facing the same way.
        GetBodyGroundPose(out Vector3 position, out Quaternion facing);
        FinishExit(position, facing);
    }

    protected override void OnEnterHiding()
    {
        if (hidingCharacter != null)
        {
            hidingCharacter.transform.SetLocalPositionAndRotation(characterStartPos, characterStartRot);

            // Turning it off and on again restarts its animation from the first frame.
            hidingCharacter.SetActive(false);
            hidingCharacter.SetActive(true);
        }

        if (doorAnim != null)
            doorAnim.SetTrigger(LockerEnterHash);
    }

    protected override void BeginExit()
    {
        IsHide = false;

        if (doorAnim != null)
            doorAnim.SetTrigger(LockerExitHash);

        // No copy to animate: come out straight away.
        if (characterAnim == null)
        {
            base.BeginExit();
            return;
        }

        // The get-out clip is imported with "Based Upon: Body Orientation / Center of Mass", so its first frame
        // puts the body at the copy's origin, facing the copy's forward. Move that origin to where the get-in clip
        // left the body, so get-out starts from that exact pose and faces out of the locker.
        // This needs the copy's get-in -> get-out transition to have a duration of 0, otherwise the blend
        // would show the get-in pose shifted to the new origin for a moment.
        GetBodyGroundPose(out Vector3 position, out Quaternion facing);
        hidingCharacter.transform.SetPositionAndRotation(position, facing);

        getInStateHash = characterAnim.GetCurrentAnimatorStateInfo(0).fullPathHash;
        characterAnim.SetTrigger(LockerExitHash);
        isExiting = true;
    }

    protected override void OnExitHiding()
    {
        if (hidingCharacter != null)
            hidingCharacter.SetActive(false);
    }

    public void SetHide(bool isHide) => IsHide = isHide;

    private bool IsCurrentStateFinished() =>
        !characterAnim.IsInTransition(0) && characterAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;

    private void GetBodyGroundPose(out Vector3 position, out Quaternion facing)
    {
        position = characterAnim.bodyPosition;
        position.y = hidingCharacter.transform.position.y;

        Vector3 forward = characterAnim.bodyRotation * Vector3.forward;
        forward.y = 0f;
        facing = forward.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(forward)
            : hidingCharacter.transform.rotation;
    }
}
