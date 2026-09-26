using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;

public class Door : Interactable
{
    private readonly int IsOpenHash = Animator.StringToHash("IsOpen");
    private Animator anim;
    private bool isOpen = false;

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
        link.enabled = isOpen;
    }

    public override void Interact(GameObject interactor)
    {
        if (currentLock != null && currentLock.IsLocked)
        {
            currentLock.Interact(interactor);
            return;
        }
        
        Transform targetPoint = GetClosestPoint(interactor.transform.position);

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

        isOpen = !isOpen;
        link.enabled = isOpen;
        anim.SetBool(IsOpenHash, isOpen);
        StartCoroutine(TurnOffCollider(colDisableWaitTime));

        Debug.Log($"door interacted by {interactor.name}");
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
}