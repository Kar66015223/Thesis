using UnityEngine;

public class ReadableItem : Item
{
    public override void Interact(GameObject interactor)
    {
        base.Interact(interactor);
        ReadableItemData data = Data as ReadableItemData;
        GameEvent.OnToggleReadUI?.Invoke(true, data.text);
    }
}