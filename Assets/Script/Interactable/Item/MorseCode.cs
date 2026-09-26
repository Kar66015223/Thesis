using UnityEngine;

public class MorseCode : Item
{
    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);

        if(interactor.TryGetComponent(out PlayerController ctrl))
        {
            ctrl.hasMorseCode = true;
            GameEvent.OnShowTips?.Invoke("Player has morse code, light comes back, can return now");
            GameEvent.OnLightsOut?.Invoke(false);
        }
    }
}