using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionHandler
{
    private readonly List<Item> items = new();
    private readonly List<IInteractable> others = new();
    private int selectedItemIndex;

    public IReadOnlyList<Item> Items => items;
    public int SelectedItemIndex => selectedItemIndex;
    public Item SelectedItem => items.Count > 0 ? items[selectedItemIndex] : null;

    public void Refresh(IReadOnlyList<IInteractable> detected, GameObject interactor, bool interactorCanInteract)
    {
        Item previouslySelected = SelectedItem;

        items.Clear();
        others.Clear();

        if (interactorCanInteract)
        {
            foreach (IInteractable candidate in detected)
            {
                if (!candidate.CanInteract(interactor))
                    continue;

                if (candidate is Item item)
                    items.Add(item);
                else
                    others.Add(candidate);
            }
        }

        int previousIndex = previouslySelected != null ? items.IndexOf(previouslySelected) : -1;
        selectedItemIndex = previousIndex >= 0
            ? previousIndex : Mathf.Clamp(selectedItemIndex, 0, Mathf.Max(0, items.Count - 1));

        SortByDistance(others, interactor.transform.position);
    }

    public void ScrollItemSelection(float scrollDelta)
    {
        if (items.Count <= 1 || scrollDelta == 0f)
            return;

        int step = scrollDelta > 0f ? -1 : 1;
        selectedItemIndex = (selectedItemIndex + step + items.Count) % items.Count;
    }

    public IInteractable GetTarget(InteractButton button)
    {
        Item selectedItem = SelectedItem;
        if (selectedItem != null && selectedItem.InteractButton == button)
            return selectedItem;

        foreach (IInteractable candidate in others)
        {
            if (candidate.InteractButton == button)
                return candidate;
        }

        return null;
    }
    
    public bool TryInteract(InteractButton button, GameObject interactor)
    {
        IInteractable target = GetTarget(button);
        if (target == null || !target.CanInteract(interactor))
            return false;

        target.Interact(interactor);
        return true;
    }
    
    private static void SortByDistance(List<IInteractable> list, Vector3 origin)
    {
        if (list.Count < 2)
            return;

        list.Sort((a, b) =>
            (a.Owner.transform.position - origin).sqrMagnitude
                .CompareTo((b.Owner.transform.position - origin).sqrMagnitude));
    }
}