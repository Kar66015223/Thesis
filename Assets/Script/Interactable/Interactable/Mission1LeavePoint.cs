using UnityEngine;

/// <summary>Returns to the hideout once the interactor carries the required item.</summary>
public class Mission1LeavePoint : Interactable
{
    [Tooltip("Item needed to leave. If empty, the item from the scene's MorseCode pickup is used.")]
    [SerializeField] private ItemData requiredItem;

    protected override void OnInteract(GameObject interactor)
    {
        ItemData required = ResolveRequiredItem();
        bool hasItem = required == null ||
            (interactor.TryGetComponent(out IItemHolder holder) && holder.HasItem(required));

        if (hasItem)
            SceneLoader.LoadScene(SceneConstants.HIDEOUT_NAME);
        else
            UIEvents.ShowTip("Don't have morse code, can't return yet.");
    }
    
    // Keeps the existing scene working until requiredItem is assigned in the Inspector.
    private ItemData ResolveRequiredItem()
    {
        if (requiredItem != null)
            return requiredItem;

        MorseCode code = FindAnyObjectByType<MorseCode>(FindObjectsInactive.Include);
        if (code != null && code.Data != null)
        {
            requiredItem = code.Data;
            Debug.LogWarning($"{name}: 'Required Item' is not assigned, using '{requiredItem.name}' from the scene. " +
                "Assign it in the Inspector.", this);
        }

        return requiredItem;
    }
}