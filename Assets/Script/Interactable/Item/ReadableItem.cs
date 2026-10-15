using UnityEngine;

public class ReadableItem : Item
{
    protected override void OnPickedUp(GameObject interactor)
    {
        if (Data is ReadableItemData readable)
            UIEvents.ToggleReadPanel(true, readable.text);
        else
            Debug.LogWarning($"{name} is a ReadableItem but its Data is not ReadableItemData.", this);
    }
}
