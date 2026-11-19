using UnityEngine;

public class Locker : Hidable
{
    private static readonly int LockerEnterHash = Animator.StringToHash("LockerEnter");
    private static readonly int LockerExitHash = Animator.StringToHash("LockerExit");

    [Tooltip("The copy of the player inside this locker that plays the get-in and get-out animations. Keep it inactive in the prefab.")]
    [SerializeField] private GameObject hidingCharacterCopy;

    private Animator doorAnim;
    private Animator characterAnim;

    private Vector3 characterStartPos;
    private Quaternion characterStartRot;

    private int getInStateHash;

    protected override bool CanExit =>
        base.CanExit && (characterAnim == null || IsCurrentStateFinished());

    void Awake()
    {
        TryGetComponent(out doorAnim);
        if (hidingCharacterCopy != null)
        {
            hidingCharacterCopy.TryGetComponent(out characterAnim);
            hidingCharacterCopy.transform.GetLocalPositionAndRotation(out characterStartPos, out characterStartRot);
        }
    }

    void Update()
    {
        if (!isExiting)
            return;

        bool getOutFinished = characterAnim.GetCurrentAnimatorStateInfo(0).fullPathHash != getInStateHash
            && IsCurrentStateFinished();
            
        if (!getOutFinished)
            return;

        isExiting = false;

        GetBodyGroundPose(out Vector3 position, out Quaternion facing);
        FinishExit(position, facing);
    }

    protected override void OnEnterHiding()
    {
        if (hidingCharacterCopy != null)
        {
            hidingCharacterCopy.transform.SetLocalPositionAndRotation(characterStartPos, characterStartRot);

            hidingCharacterCopy.SetActive(false);
            hidingCharacterCopy.SetActive(true);
        }

        if (doorAnim != null)
            doorAnim.SetTrigger(LockerEnterHash);
    }

    protected override void BeginExit()
    {
        IsHide = false;

        if (doorAnim != null)
            doorAnim.SetTrigger(LockerExitHash);

        if (characterAnim == null)
        {
            base.BeginExit();
            return;
        }

        GetBodyGroundPose(out Vector3 position, out Quaternion facing);
        hidingCharacterCopy.transform.SetPositionAndRotation(position, facing);

        getInStateHash = characterAnim.GetCurrentAnimatorStateInfo(0).fullPathHash;
        characterAnim.SetTrigger(LockerExitHash);
        isExiting = true;
    }

    protected override void OnExitHiding()
    {
        if (hidingCharacterCopy != null)
            hidingCharacterCopy.SetActive(false);
    }

    private bool IsCurrentStateFinished() =>
        !characterAnim.IsInTransition(0) && characterAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;

    private void GetBodyGroundPose(out Vector3 position, out Quaternion facing)
    {
        position = characterAnim.bodyPosition;
        position.y = hidingCharacterCopy.transform.position.y;

        Vector3 forward = characterAnim.bodyRotation * Vector3.forward;
        forward.y = 0f;
        facing = forward.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(forward)
            : hidingCharacterCopy.transform.rotation;
    }
}
