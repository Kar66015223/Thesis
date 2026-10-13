using System.Collections;
using UnityEngine.AI;
using UnityEngine;
using Unity.AI.Navigation;

public class Door : Interactable
{
    private readonly int IsOpenHash = Animator.StringToHash("IsOpen");
    private Animator anim;

    private bool isOpen = false;
    public bool IsOpen => isOpen;
    private Lock currentLock;

    [SerializeField] private Transform playerPointFront;
    [SerializeField] private Transform playerPointBack;

    private Collider col;
    [SerializeField] private float colDisableWaitTime;

    [SerializeField] private NavMeshLink link;

    protected override void Awake()
    {
        base.Awake();
        Owner = gameObject;
        anim = GetComponentInChildren<Animator>();
        col = GetComponent<Collider>();
        currentLock = GetComponentInChildren<Lock>();

        if(link != null)
            link.enabled = isOpen;
    }

    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);

        if (currentLock != null && currentLock.IsLocked)
        {
            currentLock.Interact(interactor);
            return;
        }

        Transform targetPoint = GetClosestPoint(interactor.transform.position);
        if (interactor.TryGetComponent(out PlayerController ctrl))
            ctrl.SetCanMove(false);

        if (interactor.TryGetComponent(out CharacterController charController))
        {
            charController.enabled = false;

            interactor.transform.position = targetPoint.position;
            
            if (charController.TryGetComponent(out PlayerController controller))
                controller.SetModelRotation(targetPoint);

            charController.enabled = true;
        }
        else
        {
            interactor.transform.position = targetPoint.position;
        }

        SetDoorOpen(!isOpen);
    }

    IEnumerator TurnOffCollider(float offTime)
    {
        if (col == null)
            yield return null;

        col.enabled = false;
        yield return new WaitForSeconds(offTime);
        col.enabled = true;
    }

    public Transform GetClosestPoint(Vector3 playerPos)
    {
        float distanceToA = Vector3.Distance(playerPos, playerPointFront.position);
        float distanceToB = Vector3.Distance(playerPos, playerPointBack.position);

        return distanceToA < distanceToB ? playerPointFront : playerPointBack;
    }

    public bool TryGetReachableApproach(
        Vector3 from,
        int areaMask,
        out Transform approachPoint)
    {
        approachPoint = null;

        bool canReachFront = TryGetPathLenght(
            from, playerPointFront, areaMask, out float frontLength);

        bool canReachBack = TryGetPathLenght(
            from, playerPointBack, areaMask, out float backLength);

        if (!canReachFront && !canReachBack)
            return false;

        if (canReachFront &&
            (!canReachBack || frontLength <= backLength))
        {
            approachPoint = playerPointFront;
        }
        else
            approachPoint = playerPointBack;

        return true;
    }

    private bool TryGetPathLenght(
        Vector3 from,
        Transform point,
        int areaMask,
        out float pathLength)
    {
        pathLength = 0f;

        if (point == null)
            return false;

        NavMeshPath path = new();
        bool calculated = NavMesh.CalculatePath(from, point.position, areaMask, path);

        if (!calculated || path.status != NavMeshPathStatus.PathComplete)
            return false;

        Vector3[] corners = path.corners;
        for (int i = 1; i < corners.Length; i++)
        {
            pathLength += Vector3.Distance(corners[i - 1], corners[i]);
        }

        return true;
    }

    public bool OpenForEnemy()
    {
        if (isOpen)
            return true;

        if (currentLock != null && currentLock.IsLocked)
            currentLock.ForceUnlock();

        SetDoorOpen(true);
        return true;
    }

    private void SetDoorOpen(bool open)
    {
        isOpen = open;

        if (link != null)
            link.enabled = true;

        anim.SetBool(IsOpenHash, isOpen);
        StartCoroutine(TurnOffCollider(colDisableWaitTime));
    }
}