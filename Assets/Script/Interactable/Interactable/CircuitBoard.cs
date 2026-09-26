using UnityEngine;

public class CircuitBoard : Interactable
{
    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);
        GameEvent.OnLightsOut?.Invoke(true);
        GameEvent.OnShowTips?.Invoke("Lights out, enemies come to check circuit board");
    }
}