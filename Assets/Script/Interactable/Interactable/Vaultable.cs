using System.Collections;
using UnityEngine;

public class Vaultable : Interactable
{
    [SerializeField] private Transform sideAPoint;
    [SerializeField] private Transform sideBPoint;
    [SerializeField] private float vaultDuration = 0.5f;
    [SerializeField] private AnimationCurve vaultCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float vaultHeight = 1.0f;
    private bool isVaulting = false;

    public override bool CanInteract(GameObject interactor)
    {
        return base.CanInteract(interactor) && !isVaulting;
    }

    public override void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor))
            return;

        base.Interact(interactor);

        StartCoroutine(VaultRoutine(interactor));
    }

    private IEnumerator VaultRoutine(GameObject interactor)
    {
        isVaulting = true;

        Transform startTransform = GetClosestPoint(interactor.transform.position);
        Transform targetTransform = (startTransform == sideAPoint) ? sideBPoint : sideAPoint;

        Vector3 startPos = interactor.transform.position;
        Vector3 targetPos = targetTransform.position;

        if (interactor.TryGetComponent(out CharacterController charController))
            charController.enabled = false;

        float elapsedTime = 0f;
        while (elapsedTime < vaultDuration)
        {
            float normalizedTime = elapsedTime / vaultDuration;
            float curveValue = vaultCurve.Evaluate(normalizedTime);

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, curveValue);
            currentPos.y += Mathf.Sin(curveValue * Mathf.PI) * vaultHeight;

            interactor.transform.position = currentPos;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        interactor.transform.position = targetPos;
        if (interactor.TryGetComponent(out CharacterController _))
            charController.enabled = true;
            
        isVaulting = false;
    }

    private Transform GetClosestPoint(Vector3 interactor)
    {
        float distanceToA = Vector3.Distance(interactor, sideAPoint.position);
        float distanceToB = Vector3.Distance(interactor, sideBPoint.position);

        return distanceToA < distanceToB ? sideAPoint : sideBPoint;
    }
}