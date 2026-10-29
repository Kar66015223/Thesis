using System.Collections;
using UnityEngine;

public class Vaultable : Interactable
{
    [SerializeField] private Transform sideAPoint;
    [SerializeField] private Transform sideBPoint;
    [SerializeField] private float vaultDuration = 0.5f;
    [SerializeField] private AnimationCurve vaultCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float vaultHeight = 1.0f;

    private bool isVaulting;

    public override InteractButton InteractButton => InteractButton.Space;
    public override bool CanInteract(GameObject interactor) => base.CanInteract(interactor) && !isVaulting;
    protected override void OnInteract(GameObject interactor) => StartCoroutine(VaultRoutine(interactor));

    private IEnumerator VaultRoutine(GameObject interactor)
    {
        isVaulting = true;

        Transform start = GetClosestPoint(interactor.transform.position);
        Vector3 startPos = interactor.transform.position;
        Vector3 targetPos = (start == sideAPoint ? sideBPoint : sideAPoint).position;

        interactor.TryGetComponent(out IInteractorBody body);
        body?.SetPhysicsEnabled(false);

        for (float elapsed = 0f; elapsed < vaultDuration; elapsed += Time.deltaTime)
        {
            float curveValue = vaultCurve.Evaluate(elapsed / vaultDuration);

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, curveValue);
            currentPos.y += Mathf.Sin(curveValue * Mathf.PI) * vaultHeight;
            interactor.transform.position = currentPos;

            yield return null;
        }

        interactor.transform.position = targetPos;
        body?.SetPhysicsEnabled(true);

        isVaulting = false;
    }
    
    private Transform GetClosestPoint(Vector3 pos)
    {
        float distToA = (pos - sideAPoint.position).sqrMagnitude;
        float distToB = (pos - sideBPoint.position).sqrMagnitude;

        return distToA < distToB ? sideAPoint : sideBPoint;
    }
}