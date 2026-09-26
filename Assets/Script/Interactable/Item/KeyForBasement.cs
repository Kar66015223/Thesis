using UnityEngine;

public class KeyForBasement : Item
{
    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);

        if(interactor.TryGetComponent(out PlayerController ctrl))
        {
            ctrl.hasKey = true;
            GameEvent.OnShowTips?.Invoke("Player has key, can unlock basement door now");
        }
    }
}