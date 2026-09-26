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

    private IEnumerator VaultRoutine(GameObject player)
    {
        isVaulting = true;

        Transform startTransform = GetClosestPoint(player.transform.position);
        Transform targetTransform = (startTransform == sideAPoint) ? sideBPoint : sideAPoint;

        Vector3 startPos = player.transform.position;
        Vector3 targetPos = targetTransform.position;

        if (player.TryGetComponent(out CharacterController charController))
            charController.enabled = false;

        float elapsedTime = 0f;
        while (elapsedTime < vaultDuration)
        {
            float normalizedTime = elapsedTime / vaultDuration;
            float curveValue = vaultCurve.Evaluate(normalizedTime);

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, curveValue);
            currentPos.y += Mathf.Sin(curveValue * Mathf.PI) * vaultHeight;

            player.transform.position = currentPos;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        player.transform.position = targetPos;
        if (player.TryGetComponent(out CharacterController _))
            charController.enabled = true;
            
        isVaulting = false;
    }

    private Transform GetClosestPoint(Vector3 playerPos)
    {
        float distanceToA = Vector3.Distance(playerPos, sideAPoint.position);
        float distanceToB = Vector3.Distance(playerPos, sideBPoint.position);

        return distanceToA < distanceToB ? sideAPoint : sideBPoint;
    }
}