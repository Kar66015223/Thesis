using UnityEngine;

public class Table : Hidable
{
    private static readonly int ExitHash = Animator.StringToHash("Exit");
    [SerializeField] private GameObject hidingCharacterCopy;

    private Animator characterAnim;

    protected override bool CanExit => base.CanExit;
    protected override bool CrouchOnEnter => true;
    protected override Quaternion? EnterFacing => hidingCharacterCopy != null ? CopyStartRotation() : null;

    private Vector3 characterStartPos;
    private Quaternion characterStartRot;

    private int getInStateHash;

    void Awake()
    {
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
    }

    protected override void BeginExit()
    {
        IsHide = false;

        if (hidingCharacterCopy == null)
        {
            base.BeginExit();
            return;
        }

        GetBodyGroundPose(out Vector3 position, out _);
        hidingCharacterCopy.transform.position = position;

        getInStateHash = characterAnim.GetCurrentAnimatorStateInfo(0).fullPathHash;
        characterAnim.SetTrigger(ExitHash);
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

    private Quaternion CopyStartRotation()
    {
        Transform parent = hidingCharacterCopy.transform.parent;
        return parent != null ? parent.rotation * characterStartRot : characterStartRot;
    }
}